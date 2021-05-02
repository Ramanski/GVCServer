using GVCServer.Data.Entities;
using ModelsLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GVCServer.Repositories
{
    public interface ITrainRepository
    {
        Task<TrainModel> AddTrainAsync(TrainModel TrainModel, string station);
        Task UpdateTrain(TrainModel trainModel);
        Task<string> GetDislocationStation(Guid trainId);

        Task DeleteLastTrainOperaion(string index, string operationCode, bool includeVagonOperations);

        Task CorrectVagons(string index, List<WagonModel> newList, DateTime timeOper, string station);

        Task<TrainModel[]> GetComingTrainsAsync(string station);

        Task<TrainModel> GetTrainModelAsync(string index);

        Task<short> GetNextOrdinal(string formStation);

        Task<Train> FindTrain(string index);

        Task UpdateTrainParameters(Train train);


    }
}
