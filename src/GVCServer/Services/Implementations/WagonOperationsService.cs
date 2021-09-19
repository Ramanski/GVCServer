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
        public async Task AttachToTrain(TrainModel train, IEnumerable<WagonModel> wagons, DateTime timeOper, string sourceStation)
        {
            var newWagOpers = _imapper.Map<List<OpVag>>(wagons);
            
            await CheckWagonOperationsToAdd(newWagOpers, sourceStation);

            foreach (OpVag vagon in newWagOpers)
            {
                vagon.LastOper = true;
                vagon.Msgid = DateTime.Now;
                vagon.Source = sourceStation;
                // TODO: solve problem with empty train in wagons
                vagon.Train = null;
                vagon.TrainId = train.Id;
                vagon.DateOper = timeOper;
                vagon.CodeOper = OperationCode.TrainComposition;
                vagon.PlanForm = train.DestinationStation;
                vagon.NumNavigation = _context.Vagon.Where(v => v.Id == vagon.Num).FirstOrDefault();
            }

            _context.AddRange(newWagOpers);
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
                    throw new RailProcessException($"Вагона {wagOper.Num} в поезде нет!");
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
            var oldComposition = await _context.OpVag.Where(op => op.TrainId == trainId && op.LastOper)
                                                              .Select(op => op.Num)
                                                             .ToListAsync();
            var attachedWagons = newComposition.Except(oldComposition).ToList();
            var detachedWagons = oldComposition.Except(newComposition).ToList();

            await AddWagonOperations(trainId, OperationCode.DetachWagons, detachedWagons, timeOper, station);
            await AddWagonOperations(trainId, OperationCode.AdditionVagons, detachedWagons, timeOper, station);
        }

        public void CheckWagonOperationsToCancel(List<string> wagons, Guid trainId, string operationCode)
        {
            var errors = new List<RailProcessException>();
            var wagonOperations = _context.OpVag
                                          .Where(wo => wo.LastOper && wagons.Contains(wo.Num))
                                          .Include(wo => wo.Train)
                                          .Select(wo => new OpVag(){
                                                        Num = wo.Num,
                                                        TrainId = wo.TrainId,
                                                        Train = wo.Train,
                                                        CodeOper = wo.CodeOper
                                          });

            foreach(OpVag wagOper in wagonOperations)
            {
                if(trainId != wagOper.TrainId)
                    errors.Add(new RailProcessException($"Вагон {wagOper.Num} в поезде {wagOper.Train.TrainNum}\n"));
                if(!operationCode.Equals(wagOper.CodeOper))
                    errors.Add(new RailProcessException($"Операция для вагона {wagOper.Num} -> {wagOper.CodeOper}\n"));
            }

            if(errors.Any())
                throw new RailProcessException("Ошибки контроля операций с вагонами", new AggregateException(errors));
        }
        public async Task<string[]> GetOriginalWagons(Guid trainGui)
        {
            return await _context.OpVag.Where(o => o.TrainId == trainGui && o.CodeOper == OperationCode.TrainComposition)
                                                      .Select(o => o.Num)
                                                      .Distinct().ToArrayAsync();
        }
        public async Task CheckWagonOperationsToAdd(List<OpVag> newWagOpers, string station)
        {
            List<RailProcessException> errors = new List<RailProcessException>();
            var wagonNums = newWagOpers.Select(nwgo => nwgo.Num).ToArray();
            var lastWagOpers = await _context.OpVag.Where(ov => wagonNums.Contains(ov.Num) && ov.LastOper).ToListAsync();

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
                    throw  new RailProcessException($"Не найдены вагоны в картотеке БД: {string.Join(',', notExistingWagons)}");
                }
            }

            foreach (var lastWgOper in lastWagOpers)
            {
                OpVag newWagOper = newWagOpers.Where(nwgo => lastWgOper.Num.Equals(nwgo.Num)).First();

                if (!lastWgOper.Source.Equals(station))
                    errors.Add(new RailProcessException($"Вагон {lastWgOper.Num} на станции {lastWgOper.Source}"));

                if (lastWgOper.TrainId != null)
                    errors.Add(new RailProcessException($"Вагон {lastWgOper.Num} в другом поезде"));

                if (lastWgOper.DateOper > newWagOper.DateOper)
                    errors.Add(new RailProcessException($"Для вагона {lastWgOper.Num} после {lastWgOper.DateOper}"));
            }
            if(errors.Any())
                throw new RailProcessException("Ошибки контроля операций с вагонами", new AggregateException(errors));

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