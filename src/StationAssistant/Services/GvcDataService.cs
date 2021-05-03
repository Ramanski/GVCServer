using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using ModelsLibrary;
using ModelsLibrary.Codes;
using StationAssistant.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace StationAssistant.Services
{
    public interface IGvcDataService
    {
        public Task<List<TrainModel>> GetArrivingTrains();

        public Task<TrainModel> GetTrainInfo(string index);

        public Task<List<string[]>> GetNextDestinationStationsAsync(List<Vagon> vagons);

        public Task SendTrainArrivedAsync(string index, DateTime timeArrived);

        public Task CancelOperation(string index, short msgCode);

        public Task SendDisbanding(string index, DateTime timeDisbanded);

        public Task SendDeparting(string index, DateTime timeDeparted);

        public Task<string[]> GetNearestScheduleRoute(int directionId, byte trainKind, int minutesOffset = 30);

        public Task<byte> GetTrainKind(string destination);

        public Task<short> GetNextOrdinal();

        public Task SendTGNL(TrainModel train);
    }

    public class GvcDataService : IGvcDataService
    {
        private HttpClient _client;
        private readonly StationStorageContext _context;
        private readonly IMapper _imapper;
        private readonly IHttpService httpService;

        public GvcDataService(StationStorageContext context, IMapper imapper, IHttpService httpService)
        {
            _context = context;
            _imapper = imapper;
            this.httpService = httpService;
        }

        public async Task<List<TrainModel>> GetArrivingTrains()
        {
            List<TrainModel> trains;

            trains = await httpService.Get<List<TrainModel>>("trains/coming");

            if(trains == null)
                throw new Exception("Нет поездов на подходе");

            foreach (TrainModel train in trains)
            {
                train.Dislocation = _context.Station.Find(train.Dislocation).Mnemonic;
            }
            return trains;
        }

        public async Task<TrainModel> GetTrainInfo(string index) => await httpService.Get<TrainModel>($"trains/{index}");

        public async Task SendTrainArrivedAsync(string index, DateTime timeArrived)
        {
            MovingMsg msgArrive = new MovingMsg(OperationCode.TrainArrival, index, timeArrived);
            await httpService.Post<object>("train", msgArrive);
        }

        public async Task<List<string[]>> GetNextDestinationStationsAsync(List<Vagon> vagons)
        {
            string[] destinations = vagons.Select(v => v.Destination).Distinct().ToArray();
            return await httpService.Post<List<string[]>>("NSI/PF", destinations);
        }

        //TODO: Change to delete action
        public async Task CancelOperation(string index, string operCode)
        {
            ConsistList cancelMsg = new ConsistList();
            await httpService.Delete<object>("Train", cancelMsg);
        }

        public async Task SendDisbanding(string index, DateTime timeDisbanded)
        {
            MovingMsg msgDisband = new MovingMsg(OperationCode.TrainDisbanding, index, timeDisbanded);
            await httpService.Post<object>("Train", msgDisband);
        }

        public async Task SendDeparting(string index, DateTime timeDeparted)
        {
            MovingMsg msgDepart = new MovingMsg(OperationCode.TrainDeparture, index, timeDeparted);
            await httpService.Post<object>("Train", msgDepart);
        }

        public async Task<string[]> GetNearestScheduleRoute(int directionId, byte trainKind, int minutesOffset = 30)
        {
            QueryBuilder query = new QueryBuilder();
            query.Add("direction", directionId.ToString());
            query.Add("kind", trainKind.ToString());
            query.Add("minsOffset", minutesOffset.ToString());

            return await httpService.Get<string[]>("NSI/Closest-Departure" + query.ToString());
        }

        public async Task<byte> GetTrainKind(string destination)
        {
            return await httpService.Get<byte>($"NSI/Train-Kind?destination={destination}");
        }

        public async Task<short> GetNextOrdinal()
        {
            return await httpService.Get<short>("Train/Next-Ordinal");
        }

        public async Task SendTGNL(TrainModel trainModel)
        {
            ConsistList consistList = new ConsistList(OperationCode.TrainComposition, trainModel, DateTime.Now);
            await httpService.Post<object>("Train", consistList);
        }

        public Task CancelOperation(string index, short msgCode)
        {
            throw new NotImplementedException();
        }
    }
}
