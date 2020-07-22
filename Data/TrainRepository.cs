using AutoMapper;
using GVCServer.Data.Entities;
using GVCServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GVCServer.Data
{
    public class TrainRepository : ITrainRepository
    {
        private readonly IVCStorageContext _context;
        private readonly IMapper _imapper;

        public TrainRepository(IVCStorageContext context, IMapper imapper)
        {
            _context = context;
            _imapper = imapper;
        }

        public async Task<bool> AddTrainAsync(TrainList trainList, string station)
        {
            Train train;
            OpTrain opTrain;
            List<OpVag> opVag;

            try
            {
                train = _imapper.Map<Train>(trainList);
                opTrain = new OpTrain
                {
                    SourceStation = station,
                    Datop = trainList.FormTime,
                    Kop = "P0005",
                    Msgid = DateTime.Now,
                    Train = train
                };

                opVag = _imapper.Map<List<OpVag>>(trainList.Vagons);
                foreach(OpVag vagon in opVag)
                {
                    vagon.Source = station;
                    vagon.Train = train;
                    vagon.CodeOper = "P0005";
                    vagon.Msgid = DateTime.Now;
                    vagon.PlanForm = _context.Station.Where(s => s.Code.StartsWith(train.DestinationNode)).Select(s => s.Code).FirstOrDefault();
                    Vagon vag = _context.Vagon
                                        .Where(v => v.Nv == vagon.VagonNum)
                                        .SingleOrDefault();
                    if (vag == null)
                        throw new KeyNotFoundException($"Не найден вагон {vagon.VagonNum} в картотеке БД");
                    else
                        vagon.Vagon = vag;
                }

                await _context.AddAsync(train);
                await _context.AddAsync(opTrain);
                await _context.AddRangeAsync(opVag);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<bool> ProcessTrain(string index, string station, DateTime timeOper, string codeOper)
        {
            Train train = await FindTrain(index);
            OpTrain arrival = new OpTrain()
            {
                Datop = timeOper,
                Kop = codeOper,
                Msgid = DateTime.Now,
                SourceStation = station,
                Train = train
            };
            await _context.OpTrain.AddAsync(arrival);
            return await _context.SaveChangesAsync() != 0;
        }

        public async Task<bool> DeleteVagonOperaions(string index, string vagonNum)
        {
            Train train;
            OpVag[] vagonOperations;

            if(string.IsNullOrEmpty(vagonNum))
            {
                train = await FindTrain(index);
                vagonOperations =  await _context.OpVag.Where(o => o.Train == train && o.LastOper).ToArrayAsync();
            }
            else
            {
                vagonOperations = await _context.OpVag.Where(o => o.VagonNum == vagonNum && o.LastOper).ToArrayAsync();
            }

            _context.OpVag.RemoveRange(vagonOperations);

            return await _context.SaveChangesAsync() != 0;
        }

        public async Task<bool> DeleteTrainOperaion(string index)
        {
            Train train;
            OpTrain trainOperation;

            train = await FindTrain(index);
            trainOperation= await _context.OpTrain.Where(o => o.Train == train && o.LastOper).FirstOrDefaultAsync();
            _context.OpTrain.Remove(trainOperation);

            return await _context.SaveChangesAsync() != 0;
        }

        public async Task<bool> DisbandTrain(string index, string station)
        {
            Train train = await FindTrain(index);
            OpVag[] vagonOperations = await _context.OpVag
                                                        .Where(o => o.Train == train && o.LastOper)
                                                        .ToArrayAsync();
            OpVag[] vagonsDisbanding = new OpVag[vagonOperations.Length];
            Array.Copy(vagonOperations, vagonsDisbanding, vagonOperations.Length);
            foreach (OpVag vagon in vagonsDisbanding)
            {
                vagon.Train = null;
                vagon.CodeOper = "P0004";
                vagon.Source = station;
                vagon.Msgid = DateTime.Now;
            }

            return await _context.SaveChangesAsync() != 0;
        }

        public async Task<bool> DetachVagons(string index, string[] vagonNums, string station)
        {
            Train train = await FindTrain(index);
            List<OpVag> oldList = await _context.OpVag
                                               .Where(o => o.Train == train && o.LastOper)
                                               .ToListAsync();
            short deltaWeight = 0;

            foreach (string vagonNum in vagonNums)
            {
                OpVag oldVagon = oldList.Where(ol => ol.VagonNum == vagonNum).FirstOrDefault();
                if (oldVagon == null)
                {
                    throw new ArgumentException($"Отцепляемый вагон {vagonNum} в поезде {index} отсутствует!");
                }
                deltaWeight += oldVagon.WeightNetto;
                OpVag newVagon = oldVagon;
                newVagon.CodeOper = "P0072";
                newVagon.Train = null;
                newVagon.Source = station;
                newVagon.Msgid = DateTime.Now;
                await _context.OpVag.AddAsync(newVagon);
            }
            train.WeightBrutto -= deltaWeight;
            train.Length -= (short)vagonNums.Length;
            _context.Train.Update(train);
            return await _context.SaveChangesAsync() != 0;
        }

        public async Task<TrainSummary[]> GetComingTrainsAsync(string station)
        {
            string targetNode = station.Substring(0, 4);
            Train[] trains = await _context.Train.Where(t => t.DestinationNode == targetNode)
                                                 .Include(t => t.OpTrain)
                                                 .Select(t => new Train { TrainNum = t.TrainNum,
                                                                          FormNode = t.FormNode,
                                                                          Ordinal = t.Ordinal,
                                                                          DestinationNode = t.DestinationNode,
                                                                          Length = t.Length,
                                                                          WeightBrutto = t.WeightBrutto,
                                                                          OpTrain = new List<OpTrain> { t.OpTrain.OrderByDescending(t => t.Msgid).FirstOrDefault() }
                                                 })
                                                 .ToArrayAsync();

            if (trains != null)
                return _imapper.Map<TrainSummary[]>(trains);
            else return null;
        }

        public async Task<TrainList> GetTrainAsync(string index)
        {
            TrainList trainList;
            Train train;
            List<OpVag> vagons;

            try
            {
                train = await FindTrain(index);
                vagons = await _context.OpVag
                                       .Where(o => o.TrainId.Equals(train.Uid) && o.LastOper)
                                       .Include(o => o.Vagon)
                                       .ToListAsync();

                trainList = _imapper.Map<TrainList>(train);
                trainList.Vagons = _imapper.Map<List<VagonModel>>(vagons);
                return trainList;
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public async Task<bool> CorrectVagons(string index, List<OpVag> newList, string station)
        {
            Train train = await FindTrain(index);
            List<OpVag> oldList = await _context.OpVag
                                               .Where(o => o.Train == train && o.LastOper)
                                               .ToListAsync();
            short deltaWeight = 0;
            short deltaLength = 0;

            // Cравнение списков вагонов
            foreach(OpVag newVagon in newList)
            {
                OpVag oldVagon = oldList.Where(ol => ol.VagonNum == newVagon.VagonNum).FirstOrDefault();
                if (oldVagon == null)
                {
                    newVagon.CodeOper = "P0071";
                    deltaWeight += newVagon.WeightNetto;
                    deltaLength++;
                }
                else
                {
                    deltaWeight += (short)(newVagon.WeightNetto - oldVagon.WeightNetto);
                    newVagon.CodeOper = "P0073";
                    newVagon.PlanForm = oldVagon.PlanForm;
                }
                newVagon.Train = train;
                newVagon.Source = station;
                newVagon.Msgid = DateTime.Now;
                await _context.OpVag.AddAsync(newVagon);
            }
            train.WeightBrutto += deltaWeight;
            train.Length += deltaLength;
            _context.Train.Update(train);
            return await _context.SaveChangesAsync() != 0;
        }

        private int[] DefineIndex(string index)
        {
            int[] trainParams = new int[3];
            string[] indexParams;
            
            indexParams = index.Split(' ', 3);
            
            if (indexParams.Length != 3 || index.Length != 13)
                throw new FormatException($"Неверно задан индекс запрашиваемого поезда: {index}");

            for(byte i=0; i<3; i++)
            {
                if(!Int32.TryParse(indexParams[i], out trainParams[i]))
                    throw new FormatException($"Невозможно определить параметр из индекса: {trainParams[i]}");
            }

            return trainParams;
        }

        private async Task<Train> FindTrain(string index)
        {
            int[] trainParams = DefineIndex(index);

            Train train = await _context.Train
                                      .Where(t => t.FormNode == trainParams[0].ToString() && t.Ordinal == trainParams[1] && t.DestinationNode == trainParams[2].ToString())
                                      .OrderByDescending(t => t.FormTime)
                                      .FirstOrDefaultAsync();
            if (train == null)
            {
                throw new KeyNotFoundException($"Не найдена информация по индексу запрашиваемого поезда: {index}");
            }
            return train;
        }

    }
}
