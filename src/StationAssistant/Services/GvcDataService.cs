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

        public Task<TrainModel> GetTrainInfo(Guid trainId);

        public Task<List<string[]>> GetNextDestinationStationsAsync(List<Vagon> vagons);

        public Task SendTrainArrivedAsync(Guid trainId, DateTime timeArrived);

        public Task CancelMovingOperation(Guid trainId, string msgCode);

        public Task SendDisbanding(Guid trainId, DateTime timeDisbanded);

        public Task SendDeparting(Guid trainId, DateTime timeDeparted);

        public Task<TrainRoute> GetNearestScheduleRoute(int directionId, byte trainKind, int minutesOffset = 30);

        public Task<TrainModel> SendTrainCompositionAsync(TrainModel train);
    }

    public class GvcDataService : IGvcDataService
    {
        private readonly StationStorageContext _context;
        private readonly IHttpService httpService;

        public GvcDataService(StationStorageContext context, IHttpService httpService)
        {
            _context = context;
            this.httpService = httpService;
        }

        public async Task<List<TrainModel>> GetArrivingTrains()
        {
            var comingTrains = await httpService.Get<List<TrainModel>>("trains/coming");

            if(comingTrains == null || !comingTrains.Any())
                throw new RailProcessException("Нет поездов на подходе");

            foreach (TrainModel train in comingTrains)
            {
                train.Dislocation = _context.Station.Find(train.Dislocation)?.Mnemonic ?? "НЕОП";
            }
            return comingTrains;
        }

        public async Task<TrainModel> GetTrainInfo(Guid trainId) => await httpService.Get<TrainModel>($"trains/{trainId.ToString()}");

        public async Task SendTrainArrivedAsync(Guid trainId, DateTime timeArrived)
        {
            MovingMsg msgArrive = new MovingMsg( OperationCode.TrainArrival, trainId, timeArrived);
            await httpService.Post<object>($"{trainId}/operations", msgArrive);
        }

        public async Task<List<string[]>> GetNextDestinationStationsAsync(List<Vagon> wagons)
        {
            string[] destinations = wagons.Select(w => w.Destination).Distinct().ToArray();
            return await httpService.Post<List<string[]>>("nsi/pf", destinations);
        }

        public async Task CancelMovingOperation(Guid trainId, string operCode)
        {
            MovingMsg movingMsg = new(operCode, trainId, DateTime.Now);
            await httpService.Delete<object>($"{trainId}/operations", movingMsg);
        }

        public async Task SendDisbanding(Guid trainId, DateTime timeDisbanded)
        {
            MovingMsg msgDisbanding = new MovingMsg(OperationCode.TrainDisbanding, trainId, timeDisbanded);
            await httpService.Post<object>($"{trainId}/operations", msgDisbanding);
        }

        public async Task SendDeparting(Guid trainId, DateTime timeDeparted)
        {
            MovingMsg msgDeparting = new MovingMsg(OperationCode.TrainDeparture, trainId, timeDeparted);
            await httpService.Post<object>($"{trainId}/operations", msgDeparting);
        }

        public async Task<TrainRoute> GetNearestScheduleRoute(int directionId, byte trainKind, int minutesOffset = 30)
        {
            QueryBuilder query = new QueryBuilder();
            query.Add("direction", directionId.ToString());
            query.Add("kind", trainKind.ToString());
            query.Add("minsOffset", minutesOffset.ToString());

            return await httpService.Get<TrainRoute>("nsi/closest-train-route" + query.ToString());
        }

        public async Task<TrainModel> SendTrainCompositionAsync(TrainModel trainModel)
        {
            ConsistList consistList = new ConsistList(OperationCode.TrainComposition, trainModel, DateTime.Now);
            return await httpService.Post<TrainModel>("trains", consistList);
        }
    }
}
