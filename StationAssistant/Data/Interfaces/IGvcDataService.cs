using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StationAssistant.Data.Entities;
using ModelsLibrary;

namespace StationAssistant.Data
{
    public interface IGvcDataService
    {
        public Task<List<TrainModel>> GetArrivingTrains();

        public Task<TrainModel> GetTrainInfo(string index);

        public Task<List<string[]>> GetNextDestinationStationsAsync(List<Vagon> vagons);

        public Task SendTrainArrivedAsync(string index, DateTime timeArrived);

        public Task CancelOperation(string index, string msgCode);

        public Task SendDisbanding(string index, DateTime timeDisbanded);

        public Task SendDeparting(string index, DateTime timeDeparted);

        public Task<string[]> GetNearestScheduleRoute(int directionId, byte trainKind, int minutesOffset = 30);

        public Task<byte> GetTrainKind(string destination);

        public Task<short> GetNextOrdinal();

        public Task SendTGNL(TrainModel train);
    }
}
