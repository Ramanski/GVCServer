using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GVCServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelsLibrary.Codes;

namespace GVCServer.Repositories
{
    public class TrainOperationsService
    {
        private readonly IVCStorageContext _context;
        private readonly ILogger _logger;
        public TrainOperationsService(IVCStorageContext context,
                                    ILogger<WagonOperationsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task CreateTrain(Guid trainId, DateTime dateOper, string station)
        {
            var opTrain = new OpTrain
            {
                SourceStation = station,
                Datop = dateOper,
                Kop = OperationCode.TrainComposition,
                Msgid = DateTime.Now,
                TrainId = trainId,
                LastOper = true
            };

            _context.Add(opTrain);
            _logger.LogInformation("Saving operation to train", opTrain);
            var affected = await _context.SaveChangesAsync();
            _logger.LogInformation($"Saved {affected} of 1 records");
        }

        public async Task ProcessTrain(Guid trainId, string station, DateTime timeOper, string operationCode)
        {
            _logger.LogInformation($"Processing operation {operationCode} for train {trainId}");

            OpTrain newOperation = new OpTrain()
            {
                Datop = timeOper,
                Kop = operationCode,
                Msgid = DateTime.Now,
                SourceStation = station,
                TrainId = trainId
            };

            _context.OpTrain.Add(newOperation);
            _logger.LogInformation("Saving operation to train", newOperation);

            var affected = await _context.SaveChangesAsync();
            _logger.LogInformation($"Saved {affected} of 1 record");
        }

        public async Task CorrectComposition(Guid trainId, DateTime timeOper, string station)
        {
            OpTrain correctOperation = new OpTrain()
            {
                Datop = timeOper,
                Kop = OperationCode.CorrectingComposition,
                Msgid = DateTime.Now,
                SourceStation = station,
                TrainId = trainId
            };

            _context.OpTrain.Add(correctOperation);
            _logger.LogInformation($"Train Operation {OperationCode.CorrectingComposition} added", correctOperation);

            var affected = await _context.SaveChangesAsync();
            _logger.LogInformation($"Saved {affected} of + 1 record");

            await UpdateTrainParameters(trainId);
        }

        public async Task UpdateTrainParameters(Guid trainId)
        {
            var train = await _context.Train.FindAsync(trainId);
            if(train == null)
                throw new Exception("Поезд не найден");

            var wagOpers = await _context.ActualWagOpers
                                            .Where(wgo => wgo.TrainId == train.Uid)
                                            .Select(op => new ActualWagonOperations(){ WeightNetto = op.WeightNetto, WagonNum = op.WagonNum })
                                            .ToListAsync();
            var wagons = wagOpers.Select(wo => wo.WagonNum);
            var taraOverall = await _context.Vagon.Where(w => wagons.Contains(w.Id)).SumAsync(w => w.Tvag);
            
            train.Length = (short) (wagOpers.Count());
            train.WeightBrutto = (short) (wagOpers.Sum(o => o.WeightNetto) + taraOverall);

            _context.Update(train);
            _logger.LogInformation("Updating train info", train);

            var affected = await _context.SaveChangesAsync();
            _logger.LogInformation($"Saved {affected} of 1 record");
        }

        public async Task DeleteLastTrainOperaion(string trainIndex, string operationCode)
        {
            var trainModel = await _context.TrainModels.Where(t => t.Index == trainIndex).FirstOrDefaultAsync();
            var trainOperation = await _context.OpTrain
                                          .Where(o => o.TrainId == trainModel.Id && (bool)o.LastOper)
                                          .FirstOrDefaultAsync();

            if (operationCode.Equals(trainOperation.Kop))
            {
                _logger.LogInformation("Removing train operation", trainOperation);
                _context.OpTrain.Remove(trainOperation);
            }
            else
            {
                var operationName = _context.Operation.Where(o => o.Code.Equals(trainOperation.Kop))
                                                      .Select(o => o.Name).FirstOrDefault();
                throw new MethodAccessException($"Последняя операция для поезда -> {trainOperation.Kop}");
            }

            var affected = await _context.SaveChangesAsync();
            _logger.LogInformation($"Removed {affected} of 1 record");
        }
    }
}