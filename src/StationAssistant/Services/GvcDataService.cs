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

        public Task CancelOperation(Guid trainId, string msgCode);

        public Task SendDisbanding(Guid trainId, DateTime timeDisbanded);

        public Task SendDeparting(Guid trainId, DateTime timeDeparted);

        public Task<string[]> GetNearestScheduleRoute(int directionId, byte trainKind, int minutesOffset = 30);

        public Task<byte> GetTrainKind(string destination); 
        public Task SendTGNL(TrainModel train);
    }

    public class GvcDataService : IGvcDataService
    {
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

        public async Task<TrainModel> GetTrainInfo(Guid trainId) => await httpService.Get<TrainModel>($"trains/{trainId.ToString()}");

        public async Task SendTrainArrivedAsync(Guid trainId, DateTime timeArrived)
        {
            MovingMsg msgArrive = new MovingMsg( OperationCode.TrainArrival, trainId, timeArrived);
            await httpService.Post<object>($"{trainId}/operations", msgArrive);
        }

        public async Task<List<string[]>> GetNextDestinationStationsAsync(List<Vagon> vagons)
        {
            string[] destinations = vagons.Select(v => v.Destination).Distinct().ToArray();
            return await httpService.Post<List<string[]>>("nsi/pf", destinations);
        }

        //TODO: Change to delete action
        public async Task CancelOperation(Guid trainId, string operCode)
        {
            ConsistList cancelMsg = new ConsistList();
            await httpService.Delete<object>("train", cancelMsg);
        }

        public async Task CancelCreation(Guid trainId)
        {
            ConsistList cancelMsg = new ConsistList();
            await httpService.Delete<object>("train", cancelMsg);
        }

        public async Task SendDisbanding(Guid trainId, DateTime timeDisbanded)
        {
            MovingMsg msgDisband = new MovingMsg(OperationCode.TrainDisbanding, trainId, timeDisbanded);
            await httpService.Post<object>($"{trainId}/operations", msgDisband);
        }

        public async Task SendDeparting(Guid trainId, DateTime timeDeparted)
        {
            MovingMsg msgDepart = new MovingMsg(OperationCode.TrainDeparture, trainId, timeDeparted);
            await httpService.Post<object>($"{trainId}/operations", msgDepart);
        }

        public async Task<string[]> GetNearestScheduleRoute(int directionId, byte trainKind, int minutesOffset = 30)
        {
            QueryBuilder query = new QueryBuilder();
            query.Add("direction", directionId.ToString());
            query.Add("kind", trainKind.ToString());
            query.Add("minsOffset", minutesOffset.ToString());

            return await httpService.Get<string[]>("nsi/closest-departure" + query.ToString());
        }

        public async Task<byte> GetTrainKind(string destination)
        {
            return await httpService.Get<byte>($"nsi/train-kind?destination={destination}");
        }

        public async Task SendTGNL(TrainModel trainModel)
        {
            ConsistList consistList = new ConsistList(OperationCode.TrainComposition, trainModel, DateTime.Now);
            await httpService.Post<object>("train", consistList);
        }
    }
}
