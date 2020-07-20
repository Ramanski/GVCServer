using AutoMapper;
using GVCServer.Data.Entities;
using GVCServer.Models;
using Microsoft.EntityFrameworkCore;
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

        public Task<bool> Appendix(string index, string station)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Arrival(string index, string station)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteTrain(string index)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Departure(string index, string station)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Disbanding(string index, string station)
        {
            throw new NotImplementedException();
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
                int[] trainParams = DefineIndex(index);

                train = await _context.Train
                                      .Where(t => t.FormNode == trainParams[0].ToString() && t.Ordinal == trainParams[1] && t.DestinationNode == trainParams[2].ToString())
                                      .OrderByDescending(t=> t.FormTime)
                                      .FirstOrDefaultAsync();
                if (train == null)
                {
                    throw new KeyNotFoundException($"Не найдена информация по индексу запрашиваемого поезда: {index}");
                }

                vagons = await _context.OpVag
                                       .Where(o => o.TrainId.Equals(train.Uid) && o.CodeOper == "P0005")
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

        public Task<bool> Proceed(string index, string station)
        {
            throw new NotImplementedException();
        }

        public Task<bool> PutTrainAsync(string index, List<Vagon> cars, string station)
        {
            throw new NotImplementedException();
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

    }
}
