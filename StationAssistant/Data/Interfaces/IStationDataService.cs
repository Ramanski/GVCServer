using StationAssistant.Data.Entities;
using ModelsLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StationAssistant.Data
{
    public interface IStationDataService
    {
        public Task<List<TrainModel>> GetDepartingTrains();

        public Task UpdatePathOccupation(int pathId);

        public Task UpdatePaths(string area);

        public Task AddTrainAsync(string index, DateTime timeArrived, int pathId);

        public Task DeleteTrainAsync(string index);

        public Task<List<TrainModel>> GetAllTrainsAsync();

        public Task<PathModel> GetPathAsync(int pathId);

        public Task<List<PathModel>> GetPaths();

        public Task<Direction[]> GetDirections();

        public Task<List<PathModel>> GetPathsOnAreaAsync(string area, bool sort);

        public Task<TrainModel> GetTrainOnPath(int pathId);

        public Task<List<Vagon>> GetVagons();

        public Task<Vagon[]> GetVagonsOnArea(string Area);

        public Task<List<Vagon>> GetVagonsOnPath(int pathId);

        public Task<List<Vagon>> GetVagonsOfTrain(string trainIndex);

        public Task<string[]> GetAreasAsync();

        public Task RelocateTrain(string trainIndex, int pathId);

        public Task TrainDeparture(string index);

        public Task<List<PathModel>> GetAvailablePaths(TrainModel train, bool arriving, bool departing);

        public Task<short> SetDepartureRoute(TrainModel trainModel);

        public Task<List<TrainModel>> GetArrivedTrainsAsync();

        public Task<List<TrainKind>> GetTrainKinds();

        public Task UpdateTrain(TrainModel trainModel);

        public Task<List<Vagon>> DisbandTrain(TrainModel train);

        public Task FormTrain(List<Vagon> vagons, byte trainKind, bool checkPFclaims);

        public void CheckPFclaimsAsync(string destination, ref List<Vagon> vagons);
    }
}
