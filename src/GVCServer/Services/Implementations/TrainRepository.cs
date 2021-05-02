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
            _logger.LogInformation("Got trainModel to create", TrainModel);

            var train = _imapper.Map<Train>(TrainModel);
            train.FormStation = _context.Station.Where(s => s.Code.StartsWith(TrainModel.Index.Substring(0, 4)))
                                                .Select(s => s.Code)
                                                .FirstOrDefault();
            train.DestinationStation = _context.Station.Where(s => s.Code.StartsWith(TrainModel.Index.Substring(9, 4)))
                                                       .Select(s => s.Code)
                                                       .FirstOrDefault();
            train.Dislocation = station;
            train.Ordinal = await GetNextOrdinal(station);

            _logger.LogDebug("Found {FormStation} {DestinationStation}", train.FormStation, train.DestinationStation);

            _logger.LogInformation("Saving train", train);
            _context.Train.Add(train);

            var result = await _context.SaveChangesAsync();
            _logger.LogInformation($"Saved {result} of 1 record");

            return train;
        }

        public async Task UpdateTrainParams(TrainModel trainModel)
        {
            _logger.LogInformation("Got trainModel to update", trainModel);
            Train train = await FindTrain(trainModel.Index);

            train = _imapper.Map<Train>(trainModel);

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
            return trainModels;
        }

        public async Task<short> GetNextOrdinal(string formStation){
            short lastOrdinal = await _context.Train
                                              .Where(t => formStation.Equals(t.FormStation))
                                              .OrderByDescending(t => t.FormTime)
                                              .Select(t => t.Ordinal)
                                              .FirstOrDefaultAsync();
            if (lastOrdinal == 0 || lastOrdinal == 999)
                return 1;
            else
                return (short)(lastOrdinal + 1);
        }

        private int[] DefineIndex(string index)
        {
            int[] trainParams = new int[3];
            string[] indexParams;

            indexParams = index.Split(' ', 3);

            if (indexParams.Length != 3 || index.Length != 13)
                throw new FormatException($"Неверно задан индекс запрашиваемого поезда: {index}");

            for (byte i = 0; i < 3; i++)
            {
                if (!Int32.TryParse(indexParams[i], out trainParams[i]))
                    throw new FormatException($"Невозможно определить параметр из индекса: {trainParams[i]}");
            }

            return trainParams;
        }

        public async Task<Train> FindTrain(string index)
        {
            int[] trainParams = DefineIndex(index);
            Train train = await _context.Train
                                      .Where(t => t.FormStation.Substring(0, 4) == trainParams[0].ToString() && t.Ordinal == trainParams[1] && t.DestinationStation.Substring(0, 4) == trainParams[2].ToString())
                                      .OrderByDescending(t => t.FormTime)
                                      .FirstOrDefaultAsync();
            if (train == null)
            {
                _logger.LogWarning("Not found train in DB context. Parameters:", string.Join(',', trainParams));
                throw new KeyNotFoundException($"Не найдена информация по индексу запрашиваемого поезда: {index}");
            }

            _logger.LogInformation("Found train", train.Uid);
            return train;
        }

            public async Task<TrainModel> GetTrainModelAsync(string trainId){
                return await _context.TrainModels.Where(t => t.Id.ToString() == trainId).FirstOrDefaultAsync();
            }
          }
}
