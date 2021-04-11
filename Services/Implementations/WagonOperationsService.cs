using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GVCServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelsLibrary;
using ModelsLibrary.Codes;

namespace GVCServer.Repositories
{
    public class WagonOperationsService
    {
        private readonly IVCStorageContext _context;
        private readonly IMapper _imapper;
        private readonly ILogger _logger;
        public WagonOperationsService(IVCStorageContext context,
                                    IMapper imapper,
                                    ILogger<WagonOperationsService> logger)
        {
            _context = context;
            _imapper = imapper;
            _logger = logger;
        }
        public IQueryable<ActualWagonOperations> GetActualWagonOperationsQuery(string[] vagonNums)
        {
            return  _context.ActualWagOpers.Where(awo => vagonNums.Contains(awo.WagonNum));
        }
        public async Task AttachToTrain(Guid trainId, List<VagonModel> wagons, DateTime timeOper, string sourceStation, string destinationStation)
        {
            var newWagOpers = _imapper.Map<List<OpVag>>(wagons);
            
            await CheckWagonOperationsToAdd(newWagOpers, sourceStation);

            foreach (OpVag vagon in newWagOpers)
            {
                vagon.LastOper = true;
                vagon.Source = sourceStation;
                vagon.TrainId = trainId;
                vagon.DateOper = timeOper;
                vagon.CodeOper = OperationCode.TrainComposition;
                vagon.PlanForm = destinationStation;
                vagon.NumNavigation = _context.Vagon.Where(v => v.Id == vagon.Num).FirstOrDefault();
            }

            _context.Add(newWagOpers);
            var affected = await _context.SaveChangesAsync();
            _logger.LogInformation($"Saved {affected} of {newWagOpers.Count()} records");
        }
        public async Task DisbandWagons(Train train, string station, DateTime timeOper)
        {
            var vagonOperations = await GetLastWagonOperationsQuery(train.Uid, false)
                                                        .Select(lvo => new OpVag
                                                        {
                                                            Destination = lvo.Destination,
                                                            Mark = lvo.Mark,
                                                            Num = lvo.Num,
                                                            WeightNetto = lvo.WeightNetto,
                                                            CodeOper = OperationCode.TrainDisbanding,
                                                            DateOper = timeOper,
                                                            Source = station,
                                                            LastOper = true
                                                        })
                                                        .ToListAsync();
            _context.OpVag.AddRange(vagonOperations);
            _logger.LogInformation("Saving operation to wagons", vagonOperations);
            var affected = await _context.SaveChangesAsync();
            _logger.LogInformation($"Saved {affected} of {vagonOperations.Count()} records");
        }
        public async Task AddWagonOperations(Guid trainId, string codeOper, List<string> wagonNums, DateTime timeOper, string station)
        {
            var wagonOperations = await _context.OpVag
                                                .AsNoTracking()
                                                .Where(op => wagonNums.Contains(op.Num))
                                                .ToListAsync();
            
            foreach(OpVag wagOper in wagonOperations)
            {
                if(codeOper == OperationCode.DetachWagons && wagOper.TrainId != trainId)
                {
                    throw new ArgumentException($"Вагона {wagOper.Num} в поезде нет!");
                }
                if(codeOper == OperationCode.AdditionVagons)
                {
                    wagOper.TrainId = trainId;
                }
                wagOper.CodeOper = codeOper;  
                wagOper.DateOper = timeOper;
                wagOper.Source = station; 
            }

            _context.OpVag.AddRange(wagonOperations);
            _logger.LogInformation($"Wagon Operations {codeOper} added", wagonOperations);

            var affected = await _context.SaveChangesAsync();
            _logger.LogInformation($"Saved {affected} of {wagonNums.Count()} records");
        }
        public async Task CorrectComposition(Guid trainId, List<string> newComposition, DateTime timeOper, string station)
        {
            var oldComposition = await _context.ActualWagOpers.Where(op => op.TrainId == trainId)
                                                              .Select(op => op.WagonNum)
                                                             .ToListAsync();
            var attachedWagons = newComposition.Except(oldComposition).ToList();
            var detachedWagons = oldComposition.Except(newComposition).ToList();

            await AddWagonOperations(trainId, OperationCode.DetachWagons, detachedWagons, timeOper, station);
            await AddWagonOperations(trainId, OperationCode.AdditionVagons, detachedWagons, timeOper, station);
        }
        async Task DeleteWagonOperations(List<ActualWagonOperations> actualWagonOperations)
        {
            var deleteWagOpers = actualWagonOperations.Select(op => new OpVag{ Uid = op.OperId });
            _context.OpVag.RemoveRange(deleteWagOpers);

            var affected = await _context.SaveChangesAsync();
            _logger.LogInformation($"Removed {affected} of {deleteWagOpers.Count()} records");

            /// !!! Не забыть обновить параметры поезда
            // if (assignedTrain != null)
            // {
            //     await trainRepository.UpdateTrainParameters(assignedTrain.First());
            // }
        }
        public void CheckWagonOperationsToCancel(List<ActualWagonOperations> wagonOperations, Guid trainId, string operationCode)
        {
            var errors = new List<Exception>();

            foreach(ActualWagonOperations wagOper in wagonOperations)
            {
                if(trainId != wagOper.TrainId)
                    errors.Add(new Exception($"Вагон {wagOper.WagonNum} в поезде {wagOper.TrainNum}\n"));
                if(!operationCode.Equals(wagOper.CodeOper))
                    errors.Add(new Exception($"Операция для вагона {wagOper.WagonNum} -> {wagOper.CodeOper}\n"));
            }

            if(errors.Any())
                throw new AggregateException(errors);
        }
        public async Task<string[]> GetOriginalWagons(Guid trainGui)
        {
            return await _context.OpVag.Where(o => o.TrainId == trainGui && o.CodeOper == OperationCode.TrainComposition)
                                                      .Select(o => o.Num)
                                                      .Distinct().ToArrayAsync();
        }
        public async Task CheckWagonOperationsToAdd(List<OpVag> newWagOpers, string station)
        {
            List<Exception> errors = new List<Exception>();
            var wagonNums = newWagOpers.Select(nwgo => nwgo.Num).ToArray();
            var lastWagOpers = await GetActualWagonOperationsQuery(wagonNums).ToListAsync();

            // Достали меньше записей по вагонам, чем заявлено в сообщении
            if (lastWagOpers.Count < newWagOpers.Count)
            {
                _logger.LogWarning("Suspected extra vagons");
                string[] existingWagons = await _context.Vagon
                                                .Where(vg => wagonNums.Contains(vg.Id))
                                                .Select(vg => vg.Id)
                                                .ToArrayAsync();
                string[] notExistingWagons = wagonNums.Except(existingWagons).ToArray();
                if (notExistingWagons.Length > 0)
                {
                    throw  new Exception($"Не найдены вагоны в картотеке БД: {string.Join(',', notExistingWagons)}");
                }
            }

            foreach (ActualWagonOperations lastWgOper in lastWagOpers)
            {
                OpVag newWagOper = newWagOpers.Where(nwgo => lastWgOper.WagonNum.Equals(nwgo.Num)).First();

                if (!lastWgOper.Station.Equals(station))
                    errors.Add(new ArgumentException($"Вагон {lastWgOper.WagonNum} на станции {lastWgOper.Station}"));

                if (lastWgOper.TrainId != null)
                    errors.Add(new ArgumentException($"Вагон {lastWgOper.WagonNum} в другом поезде"));

                if (lastWgOper.DateOper > newWagOper.DateOper)
                    errors.Add(new ArgumentException($"Для вагона {lastWgOper.WagonNum} после {lastWgOper.DateOper}"));
            }
            if(errors.Any())
                throw new AggregateException(errors);
        }
        public IQueryable<OpVag> GetLastWagonOperationsQuery(Guid trainGuid, bool includeVagonParams)
        {
            var lastOperations = _context.OpVag.Where(o => o.TrainId == trainGuid && (bool)o.LastOper);
            if (includeVagonParams)
            {
                return lastOperations.Include(o => o.NumNavigation);
            }
            else
            {
                return lastOperations;
            }
        }
    }
}