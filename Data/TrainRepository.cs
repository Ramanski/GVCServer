﻿using AutoMapper;
using GVCServer.Data.Entities;
using GVCServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Internal;
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
            OpVag[] opVag;
            string planForm;
            List<Exception> errors;
            const string sentTGNL = "P0005";

            try
            {
                train = _imapper.Map<Train>(trainList);
                train.FormStation = _context.Station.Where(s => s.Code.StartsWith(train.FormStation)).Select(s => s.Code).FirstOrDefault();
                train.DestinationStation = _context.Station.Where(s => s.Code.StartsWith(train.DestinationStation)).Select(s => s.Code).FirstOrDefault();
                train.Dislocation = station;
                opVag = _imapper.Map<OpVag[]>(trainList.Vagons);

                errors = await CheckValuesForTGNL(station, opVag);
                if (errors.Any())
                    throw new AggregateException(errors);

                opTrain = new OpTrain
                {
                    SourceStation = station,
                    Datop = trainList.FormTime,
                    Kop = sentTGNL,
                    Msgid = DateTime.Now,
                    Train = train,
                    LastOper = true
                };

                planForm = _context.Station
                                    .Where(s => s.Code.StartsWith(train.DestinationStation))
                                    .Select(s => s.Code)
                                    .FirstOrDefault();
                if (planForm == null)
                    throw new ArgumentException($"Не найдена станция назначения по коду ЕСР {train.DestinationStation}");

                foreach (OpVag vagon in opVag)
                {
                    vagon.LastOper = true;
                    vagon.Source = station;
                    vagon.Train = train;
                    vagon.DateOper = trainList.FormTime;
                    vagon.CodeOper = sentTGNL;
                    vagon.PlanForm = planForm;
                    vagon.NumNavigation = _context.Vagon.Where(v => v.Id == vagon.Num).FirstOrDefault();
                }

                train.OpTrain.Add(opTrain);
                train.OpVag = opVag;
                _context.Train.Add(train);
                return await _context.SaveChangesAsync() != 0;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<bool> ProcessTrain(string index, string station, DateTime timeOper, string messageCode)
        {
            Train train;
            Operation[] operations;

            train = await FindTrain(index);
            operations = await GetOperations(messageCode);

            OpTrain newOperation = new OpTrain()
            {
                Datop = timeOper,
                Kop = operations.First().Code,
                Msgid = DateTime.Now,
                SourceStation = station,
                Train = train
            };
            await _context.OpTrain.AddAsync(newOperation);
            train.Dislocation = station;
            if (messageCode.Equals("203"))
            {
                return await DisbandVagons(train, station, timeOper, messageCode);
            }
            return await _context.SaveChangesAsync() != 0;
        }

        public async Task<bool> UpdateTrainNum(string index, short trainNum)
        {
            Train train = await FindTrain(index);
            train.TrainNum = trainNum.ToString();
            _context.Train.Update(train);
            return (await _context.SaveChangesAsync()) != 0;
        }

        public async Task<bool> DeleteLastVagonOperaions(string index, string messageCode)
        {
            Train train = await FindTrain(index);
            string[] vagonNums = await GetLastVagonOperationsQuery(train, false)
                                                                    .Select(vo => vo.Num)
                                                                    .ToArrayAsync();
            return await DeleteLastVagonOperaions(vagonNums, messageCode);
        }

        public async Task<bool> DeleteLastVagonOperaions(string[] vagonNums, string messageCode)
        {
            OpVag[] currentVagonOperations;
            Operation[] operationTypesToDelete;
            List<Exception> errors = new List<Exception>();

            currentVagonOperations = await GetLastVagonOperationsQuery(vagonNums, false)
                                                                    .Include(vo => vo.CodeOperNavigation)
                                                                    .Include(vo => vo.Train)
                                                                    .ToArrayAsync();
            operationTypesToDelete = await _context.Operation
                                                   .Where(o => o.Message.Equals(messageCode))
                                                   .ToArrayAsync();

            var assignedTrain = currentVagonOperations.Select(o => o.Train).Distinct();
            if (assignedTrain.Count() > 1)
                throw new ArgumentException("Один или несколько вагонов находятся в разных поездах");

            foreach (OpVag vagonOperation in currentVagonOperations)
            {
                if (!operationTypesToDelete.Contains(vagonOperation.CodeOperNavigation))
                {
                    errors.Add(new MethodAccessException($"Последняя операция для вагона {vagonOperation.Num} - {vagonOperation.CodeOperNavigation.Name}\n"));
                }
            }
            if (errors.Any())
                throw new AggregateException("Возникли ошибки при обработке вагонов:\n", errors);

            _context.OpVag.RemoveRange(currentVagonOperations);
            await _context.SaveChangesAsync();

            if (assignedTrain != null)
            {
                await UpdateTrainParameters(assignedTrain.First());
            }
            return true;
        }

        public async Task<bool> DeleteLastTrainOperaion(string index, string messageCode, bool includeVagonOperations)
        {
            Train train;
            OpTrain trainOperation;
            Operation[] operationTypesToDelete;
            OpVag[] vagonOperations;

            train = await FindTrain(index);
            trainOperation= await _context.OpTrain
                                          .Where(o => o.Train == train && o.LastOper)
                                          .Include(o => o.KopNavigation)
                                          .FirstOrDefaultAsync();
            operationTypesToDelete = await _context.Operation
                                                   .Where(o => o.Message.Equals(messageCode))
                                                   .ToArrayAsync();

            if (operationTypesToDelete.Contains(trainOperation.KopNavigation))
            {
                _context.OpTrain.Remove(trainOperation);
                if (trainOperation.Kop == "P0005")
                    _context.Train.Remove(train);
            }
            else
            {
                throw new MethodAccessException($"Последняя операция для поезда {index} - {trainOperation.KopNavigation.Name}");
            }

            if(includeVagonOperations)
            {
                List<Exception> errors = new List<Exception>();
                vagonOperations = await GetLastVagonOperationsQuery(train, false)
                                                    .Include(vo => vo.CodeOperNavigation)
                                                    .ToArrayAsync();
                foreach (OpVag vagonOperation in vagonOperations)
                {
                    if (!operationTypesToDelete.Contains(vagonOperation.CodeOperNavigation))
                    {
                        errors.Add(new MethodAccessException($"Последняя операция для вагона {vagonOperation.Num} - {vagonOperation.CodeOperNavigation.Name}\n"));
                    }
                }
                if (errors.Any())
                    throw new AggregateException("Возникли ошибки при обработке вагонов:\n", errors);
                _context.OpVag.RemoveRange(vagonOperations);
            }

            return await _context.SaveChangesAsync() != 0;
        }

        public async Task<bool> DisbandVagons(Train train, string station, DateTime timeOper, string messageCode)
        {
            Operation operation = (await GetOperations(messageCode)).First();
            List<OpVag> vagonOperations = await GetLastVagonOperationsQuery(train, false)
                                                        .Select(lvo => new OpVag
                                                        {
                                                            Destination = lvo.Destination,
                                                            Mark = lvo.Mark,
                                                            Num = lvo.Num,
                                                            WeightNetto = lvo.WeightNetto,
                                                            CodeOper = operation.Code,
                                                            DateOper = timeOper,
                                                            Source = station,
                                                            LastOper = true
                                                        })
                                                        .ToListAsync();
            _context.OpVag.AddRange(vagonOperations);
            return await _context.SaveChangesAsync() != 0;
        }

        public async Task<bool> DetachVagons(string index, string[] vagonNums, DateTime timeOper, string station)
        {
            Train train = await FindTrain(index);
            List<OpVag> vagonOperations = await GetLastVagonOperationsQuery(train, false).ToListAsync();
            string detachCode = GetOperations("9").Result.Where(o => o.Parameter.Equals(0)).FirstOrDefault().Code;

            foreach (string vagonNum in vagonNums)
            {
                OpVag vagOper = vagonOperations.Where(ol => ol.Num == vagonNum)
                                        .Select(ol => new OpVag {
                                            Destination = ol.Destination,
                                            Mark = ol.Mark,
                                            Num = ol.Num,
                                            WeightNetto = ol.WeightNetto,
                                            CodeOper = detachCode,
                                            DateOper = timeOper,
                                            Source = station,
                                            LastOper = true
                                        })
                                        .FirstOrDefault();
                if (vagOper == null)
                {
                    throw new ArgumentException($"Отцепляемый вагон {vagonNum} в поезде {index} отсутствует!");
                }
                await _context.OpVag.AddAsync(vagOper);
            }

            OpTrain newOperation = new OpTrain()
            {
                Datop = timeOper,
                Kop = "P0073",
                Msgid = DateTime.Now,
                SourceStation = station,
                Train = train
            };

            await _context.OpTrain.AddAsync(newOperation);
            await _context.SaveChangesAsync();       
            return await UpdateTrainParameters(train);
        }

        public async Task<bool> CorrectVagons(string index, List<VagonModel> newList, DateTime timeOper, string station)
        {
            Train train = await FindTrain(index);
            List<OpVag> oldList = await GetLastVagonOperationsQuery(train, false).ToListAsync();
            string planForm = _context.Station
                                        .Where(s => s.Code.StartsWith(train.DestinationStation))
                                        .Select(s => s.Code)
                                        .FirstOrDefault();
            string attachCode = GetOperations("9").Result.Where(o => o.Parameter.Equals(1)).FirstOrDefault().Code;
            string correctCode = GetOperations("9").Result.Where(o => o.Parameter.Equals(2)).FirstOrDefault().Code;

            foreach (VagonModel correctedVagon in newList)
            {
                OpVag oldVagon;
                OpVag newVagon;
                
                newVagon = new OpVag()
                {
                    LastOper = true,
                    Source = station,
                    Train = train,
                    DateOper = timeOper,
                    PlanForm = planForm,
                    NumNavigation = _context.Vagon.Where(v => correctedVagon.Num.Equals(v.Id)).FirstOrDefault(),
                    Destination = correctedVagon.Destination,
                    WeightNetto = correctedVagon.WeightNetto,
                    SequenceNum = correctedVagon.SequenceNum,
                    Mark = correctedVagon.Mark
                };

                oldVagon = oldList.Where(ol => correctedVagon.Num.Equals(ol.Num))
                                  .FirstOrDefault();

                if (oldVagon == null)
                {
                    // Вагон прицепливается
                    newVagon.CodeOper = attachCode;
                }
                else
                {
                    // Вагон корректируется
                    newVagon.CodeOper = correctCode;
                }

                await _context.OpVag.AddAsync(newVagon);
            }
            
            OpTrain newOperation = new OpTrain()
            {
                Datop = timeOper,
                Kop = "P0073",
                Msgid = DateTime.Now,
                SourceStation = station,
                Train = train
            };

            await _context.OpTrain.AddAsync(newOperation);
            await _context.SaveChangesAsync();
            return await UpdateTrainParameters(train);
        }
        
        public async Task<TrainSummary[]> GetComingTrainsAsync(string station)
        {
            string targetNode = station.Substring(0, 4);
            Train[] trains = await _context.Train.Where(t => targetNode.Equals(t.DestinationStation) && !t.Dislocation.Equals(station))
                                                 .Include(t => t.OpTrain)
                                                    .ThenInclude(o => o.KopNavigation)
                                                 .Select(t => new Train { TrainNum = t.TrainNum,
                                                                          FormStation = t.FormStation,
                                                                          Ordinal = t.Ordinal,
                                                                          DestinationStation = t.DestinationStation,
                                                                          Length = t.Length,
                                                                          Dislocation = t.Dislocation,
                                                                          WeightBrutto = t.WeightBrutto,
                                                                          OpTrain = new List<OpTrain> { t.OpTrain.Where(o => o.LastOper).FirstOrDefault() }
                                                 })
                                                 .ToArrayAsync();

                return _imapper.Map<TrainSummary[]>(trains);
        }

        public async Task<TrainList> GetTrainListAsync(string index)
        {
            TrainList trainList;
            Train train;
            OpVag[] vagons;

            try
            {
                train = await FindTrain(index);
                vagons = await GetLastVagonOperationsQuery(train, true).ToArrayAsync();

                trainList = _imapper.Map<TrainList>(train);
                trainList.Vagons = _imapper.Map<List<VagonModel>>(vagons);
                return trainList;
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        private IQueryable<OpVag> GetLastVagonOperationsQuery(Train train, bool includeVagonParams)
        {
            var lastOperations = _context.OpVag.Where(o => o.Train == train && o.LastOper);
            if (includeVagonParams)
            {
                return lastOperations.Include(o => o.NumNavigation);
            }
            else
            {
                return lastOperations;
            }
        }

        private IQueryable<OpVag> GetLastVagonOperationsQuery(string[] vagonNums, bool includeVagonParams)
        {
            var lastOperations = _context.OpVag
                                         .Where(o => vagonNums.Contains(o.Num) && o.LastOper);
            var result = lastOperations.ToArray();
            if (includeVagonParams)
            {
                return lastOperations.Include(o => o.NumNavigation);
            }
            else
            {
                return lastOperations;
            }
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
                                      .Where(t => t.FormStation == trainParams[0].ToString() && t.Ordinal == trainParams[1] && t.DestinationStation == trainParams[2].ToString())
                                      .OrderByDescending(t => t.FormTime)
                                      .FirstOrDefaultAsync();
            if (train == null)
            {
                throw new KeyNotFoundException($"Не найдена информация по индексу запрашиваемого поезда: {index}");
            }
            return train;
        }
        
        private async Task<bool> UpdateTrainParameters(Train train)
        {
            OpVag[] vagons = await GetLastVagonOperationsQuery(train, true).ToArrayAsync();
            train.Length = (short)vagons.Length;
            train.WeightBrutto = (short)(vagons.Sum(o => o.WeightNetto) + vagons.Sum(o => o.NumNavigation.Tvag));
            _context.Update(train);
            return await _context.SaveChangesAsync() != 0;
        }

        private async Task<List<Exception>> CheckValuesForTGNL (string station, OpVag[] currentVgOpers)
        {
                List<Exception> errors = new List<Exception>();
            try
            {
                string[] vagonOperNums = currentVgOpers.Select(vo => vo.Num)
                                                       .ToArray();
                OpVag[] lastVgOpers = await GetLastVagonOperationsQuery(vagonOperNums, false).Select(vo => new OpVag
                {
                    Num = vo.Num, 
                    Source = vo.Source,
                    DateOper = vo.DateOper,
                    TrainId = vo.TrainId
                })
                                                                                        .ToArrayAsync();
                // Проверка наличия вагонов
                if (lastVgOpers.Count() < currentVgOpers.Count())
                {
                    string[] vagonCardsNums = _context.Vagon
                                                   .Where(vg => vagonOperNums.Contains(vg.Id))
                                                   .Select(vg => vg.Id)
                                                   .ToArray();
                        if (vagonCardsNums.Length != vagonOperNums.Length)
                        {
                        string[] notFoundVagons = vagonOperNums.Except(vagonCardsNums).ToArray();
                        errors.Add(new KeyNotFoundException($"Не найдены вагоны в картотеке БД: {string.Join(',', notFoundVagons)}"));
                        }
                }

                if (!lastVgOpers.Any())
                    return errors;
                foreach (OpVag lastVgOper in lastVgOpers)
                {
                    OpVag currentVgOper = currentVgOpers.Where(vgo => lastVgOper.Num.Equals(vgo.Num)).FirstOrDefault();

                    if (!lastVgOper.Source.Equals(station))
                        errors.Add(new ArgumentException($"Вагон {lastVgOper.Num} на станции {lastVgOper.Source}"));

                    if (lastVgOper.TrainId != null)
                        errors.Add(new ArgumentException($"Вагон {lastVgOper.Num} уже в другом поезде"));

                    if (lastVgOper.DateOper > currentVgOper.DateOper)
                        errors.Add(new ArgumentException($"Время последней операции {lastVgOper.DateOper} для вагона {lastVgOper.Num} позже указанной {currentVgOper.DateOper}"));

                }
            }
            catch(Exception exc)
            {
                errors.Add(exc);
            }
            return errors;
        }

        private async Task<Operation[]> GetOperations(string messageCode)
        {
            Operation[] operationCode;

            operationCode = await _context.Operation
                                            .Where(o => messageCode.Equals(o.Message))
                                            .ToArrayAsync();
            if (operationCode.Length == 0)
                throw new NullReferenceException($"Не найдено ни одной операции по сообщению {messageCode}");

            return operationCode;
        }
    }
}
