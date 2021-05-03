using AutoMapper;
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

        public async Task<Train> AddTrainAsync(TrainModel TrainModel, string station)
        {
            _logger.LogInformation("Got trainModel to create " + TrainModel);

            var train = _imapper.Map<Train>(TrainModel);
            train.Dislocation = station;
            train.Ordinal = await GetNextOrdinal(station);

            _logger.LogInformation("Saving train", train);
            _context.Train.Add(train);

            var result = await _context.SaveChangesAsync();
            _logger.LogInformation($"Saved {result} of 1 record");

            return train;
        }

        public async Task UpdateTrainParams(TrainModel actualTrainModel)
        {
            _logger.LogInformation("Got trainModel to update " + actualTrainModel);
            Train train = await FindTrain(actualTrainModel.Id);

            train = _imapper.Map<Train>(actualTrainModel);

            _context.Train.Update(train);

            var affected = await _context.SaveChangesAsync();
            _logger.LogInformation($"Saved {affected} of 1 record");
        }
        public async Task<TrainModel[]> GetComingTrainsAsync(string station)
        {
            TrainModel[] trainModels = await _context.Train.Where(t => station.Equals(t.DestinationStation) && !t.Dislocation.Equals(station))
                                                 .Include(t => t.OpTrain)
                                                    .ThenInclude(o => o.KopNavigation)
                                                 .Select(t => new TrainModel
                                                 {
                                                     Id = t.Uid,
                                                     Kind = t.TrainKindId ?? 0,
                                                     Num = t.TrainNum,
                                                     FormStation = t.FormStation,
                                                     Ordinal = t.Ordinal,
                                                     DestinationStation = t.DestinationStation,
                                                     Length = t.Length,
                                                     WeightBrutto = t.WeightBrutto,
                                                     DateOper = t.OpTrain.Where(o => (bool)o.LastOper).First().Datop
                                                 })
                                                 .ToArrayAsync();

            foreach (TrainModel trainModel in trainModels)
            {
                StickScheduleTime(trainModel);
            }
            return trainModels;
        }

        private async void StickScheduleTime(TrainModel trainModel)
        {
            var timeToArrive = _context.Schedule
                              .Where(s => s.TrainNum.ToString() == trainModel.Num)
                              .Select(s => s.ArrivalTime)
                              .FirstOrDefault();
            if (timeToArrive.HasValue)
            {
                var dateTime = DateTime.Today.AddTicks(timeToArrive.Value.Ticks);
                trainModel.DateOper = (dateTime > DateTime.Now ? dateTime : dateTime.AddDays(1));
            }
        }

        private async Task<short> GetNextOrdinal(string formStation){
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
                throw new KeyNotFoundException($"Не найдена информация по поезду");
            }

            _logger.LogInformation("Found train", train.Uid);
            return train;
        }

        public async Task<TrainModel> GetActualTrainAsync(Guid trainId){
            return await _context.TrainModels.Where(t => t.Id == trainId).FirstOrDefaultAsync();
        }
    }
}
