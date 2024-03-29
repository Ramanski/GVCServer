﻿using AutoMapper;
using GVCServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelsLibrary;
using ModelsLibrary.Codes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GVCServer.Repositories
{
    public class TrainRepository
    {
        private readonly IVCStorageContext _context;
        private readonly IMapper _imapper;
        private readonly ILogger _logger;
        public TrainRepository(IVCStorageContext context,
                              IMapper imapper,
                              ILogger<TrainRepository> logger
                              )
        {
            _context = context;
            _imapper = imapper;
            _logger = logger;
        }

        public async Task<TrainModel> AddTrainAsync(TrainModel trainModel, string station)
        {
            _logger.LogInformation("Got trainModel to create {0}", trainModel);

            var train = Train.FromTrainModel(trainModel);
            train.Dislocation = station;
            train.Ordinal = await GetNextOrdinal(station);
            train.OpTrain = new List<OpTrain>(){ new OpTrain(){ Datop = trainModel.DateOper, 
                                                                Kop = OperationCode.TrainComposition,
                                                                SourceStation = station } };

            _logger.LogInformation("Creating train {0}", train);
            _context.Add(train);

            var result = await _context.SaveChangesAsync();
            _logger.LogInformation($"Saved {result} of 1 record");

            return _imapper.Map<TrainModel>(train);
        }

        public async Task CancelTrainCreation(Guid trainId, string station)
        {
            var train = await _context.Train
                                               .Include(t => t.OpTrain
                                                                .Where(ot => ot.LastOper))
                                               .Include(t => t.OpVag
                                                                .Where(ov => ov.LastOper))
                                          .Where(t => t.Uid == trainId)
                                          .FirstOrDefaultAsync();

            if (train.OpTrain.FirstOrDefault().Kop == OperationCode.TrainComposition &&
               train.OpVag.All(ov => ov.CodeOper == OperationCode.TrainComposition) &&
               train.FormStation == station)
            {
                _context.Remove(train);
                _logger.LogInformation("Canceling creation of train {0}", train);
                var affected = await _context.SaveChangesAsync();
                _logger.LogInformation("Saved {0} of {1} records", affected, train.OpVag.Count + 2);
            }
            else
            {
                throw new RailProcessException("Отмена не возможна. Поезд ушел");
            }
        }
        public async Task UpdateTrainParams(TrainModel actualTrainModel)
        {
            _logger.LogInformation("Got train to update " + actualTrainModel);
            Train train = await FindTrain(actualTrainModel.Id);

            train.TrainNum = actualTrainModel.Num;
            train.DestinationStation = actualTrainModel.DestinationStation;
            train.TrainKindId = actualTrainModel.Kind;

            _context.Train.Update(train);

            var affected = await _context.SaveChangesAsync();
            _logger.LogInformation($"Saved {affected} of 1 record");
        }
        public async Task<TrainModel[]> GetComingTrainsAsync(string station)
        {
            TrainModel[] trainModels = await _context.Train
                                                 .Where(t => station.Equals(t.DestinationStation))
                                                 .Include(t => t.OpTrain.Where(ot => ot.LastOper))
                                                    .ThenInclude(op => op.KopNavigation)
                                                 .Select(t => new TrainModel
                                                 {
                                                     Id = t.Uid,
                                                     Kind = t.TrainKindId,
                                                     Num = t.TrainNum,
                                                     FormStation = t.FormStation,
                                                     Ordinal = t.Ordinal,
                                                     DestinationStation = t.DestinationStation,
                                                     Dislocation = t.OpTrain.Where(ot => ot.LastOper).First().SourceStation,
                                                     Length = t.Length,
                                                     CodeOper = t.OpTrain.Where(ot => ot.LastOper).First().KopNavigation.Mnemonic,
                                                     WeightBrutto = t.WeightBrutto,
                                                     DateOper = t.OpTrain.Where(ot => ot.LastOper).First().Datop
                                                 })
                                                 .Where(t => t.Dislocation != station)
                                                 .ToArrayAsync();
            return trainModels;
        }

        private async Task<short> GetNextOrdinal(string formStation)
        {
            short lastOrdinal = await _context.Train
                                              .Where(t => formStation.Equals(t.FormStation))
                                              .OrderByDescending(t => t.FormTime)
                                              .Select(t => t.Ordinal)
                                              .FirstOrDefaultAsync();
            return (lastOrdinal == 0 || lastOrdinal == 999) ? (short)1 : (short)(lastOrdinal + 1);
        }

        public async Task<Train> FindTrain(Guid trainId)
        {
            Train train = await _context.Train.FindAsync(trainId);

            if (train == null)
            {
                throw new RailProcessException($"Не найдена информация по поезду");
            }

            _logger.LogInformation("Found train", train.Uid);
            return train;
        }

        public async Task<TrainModel> GetActualTrainAsync(Guid trainId)
        {
            var trainModel = await _context.Train
                                    .Include(t => t.OpVag.Where(ov => ov.LastOper))
                                    .ThenInclude(ov => ov.NumNavigation)
                                    .Include(t => t.OpTrain.Where(ot => ot.LastOper))
                                    .Where(t => t.Uid == trainId)
                                    .Select(t => new TrainModel
                                    {
                                        Id = t.Uid,
                                        CodeOper = t.OpTrain.FirstOrDefault().Kop,
                                        DateOper = t.OpTrain.FirstOrDefault().Datop,
                                        DestinationStation = t.DestinationStation,
                                        Dislocation = t.Dislocation,
                                        FormStation = t.FormStation,
                                        Kind = t.TrainKindId,
                                        Length = t.Length,
                                        Num = t.TrainNum,
                                        Ordinal = t.Ordinal,
                                        WeightBrutto = t.WeightBrutto,
                                        Wagons = t.OpVag
                                                    .Select(wagon => new WagonModel()
                                                    {
                                                        Destination = wagon.Destination,
                                                        Kind = wagon.NumNavigation.Kind,
                                                        Ksob = wagon.NumNavigation.Ksob,
                                                        Num = wagon.Num,
                                                        Mark = wagon.Mark,
                                                        SequenceNum = wagon.SequenceNum ?? 0,
                                                        Tvag = wagon.NumNavigation.Tvag,
                                                        WeightNetto = wagon.WeightNetto ?? 0
                                                    })
                                    })
                                    .FirstOrDefaultAsync();
            return trainModel;
        }
    }
}
