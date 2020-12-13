using StationAssistant.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ModelsLibrary;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Internal;
using System.Text.Json;
using System.Net;
using System.Data.Common;
using System.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;

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

        public GvcDataService(IServiceProvider provider, StationStorageContext context, IMapper imapper)
        {
            _context = context;
            _imapper = imapper;
            _client = (HttpClient) provider.GetService(typeof(HttpClient));
        }

        private async Task<T> GetFromServer<T>(string requestUri)
        {
            T result;
            HttpResponseMessage response;
            try
            {
                response = await _client.GetAsync(requestUri);
            }
            catch(HttpRequestException)
            {
                throw new HttpRequestException("Возникла проблема связи с сервером ГВЦ");
            }
            if (response.StatusCode != HttpStatusCode.OK)
            {
                string exception = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(exception);
            }

            result = await response.Content.ReadFromJsonAsync<T>();
            return result;
        }

        private async Task PostToServer(string requestUri, MsgModel message)
        {
            HttpResponseMessage response = await _client.PostAsJsonAsync(requestUri, message);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                string exception = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(exception);
            }
        }

        private async Task<TResult> PostToServer<TResult,Src>(string requestUri, Src instance)
        {
            TResult result;
            HttpResponseMessage response = await _client.PostAsJsonAsync(requestUri, instance);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                string exception = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(exception);
            }

            result = await response.Content.ReadFromJsonAsync<TResult>();
            return result;
        }

        public async Task<List<TrainModel>> GetArrivingTrains()
        {
            List<TrainModel> trains;

            trains = await GetFromServer<List<TrainModel>>("Train/Arriving");
            
            foreach(TrainModel train in trains)
            {
                train.Dislocation = _context.Station.Find(train.Dislocation).Mnemonic;
            }
            return trains;
        }

        public async Task<TrainModel> GetTrainInfo(string index)
        {
            return await GetFromServer<TrainModel>("Train/" + index);
        }

        public async Task SendTrainArrivedAsync(string index, DateTime timeArrived)
        {
            MovingMsg msgArrive = new MovingMsg(201, index, timeArrived);
            await PostToServer("Train", msgArrive);
        }

        public async Task<List<string[]>> GetNextDestinationStationsAsync(List<Vagon> vagons)
        {
            string[] destinations = vagons.Select(v => v.Destination).Distinct().ToArray();
            return await PostToServer<List<string[]>, string[]>("NSI/PF", destinations);
        }

        public async Task CancelOperation(string index, short msgCode)
        {
            CancelMsg cancelMsg = new CancelMsg(index, msgCode);
            await PostToServer("Train", cancelMsg);
        }

        public async Task SendDisbanding(string index, DateTime timeDisbanded)
        {
            MovingMsg msgDisband = new MovingMsg(203, index, timeDisbanded);
            await PostToServer("Train", msgDisband);
        }

        public async Task SendDeparting(string index, DateTime timeDeparted)
        {
            MovingMsg msgDepart = new MovingMsg(200, index, timeDeparted);
            await PostToServer("Train", msgDepart);
        }

        public async Task<string[]> GetNearestScheduleRoute(int directionId, byte trainKind, int minutesOffset = 30)
        {
            QueryBuilder query = new QueryBuilder();
            query.Add("direction", directionId.ToString());
            query.Add("kind", trainKind.ToString());
            query.Add("minsOffset", minutesOffset.ToString());
            
            return await GetFromServer<string[]>("NSI/Closest-Departure" + query.ToString());
        }

        public async Task<byte> GetTrainKind(string destination)
        {
            return await GetFromServer<byte>($"NSI/Train-Kind?destination={destination}");
        }

        public async Task<short> GetNextOrdinal()
        {
            return await GetFromServer<short>("Train/Next-Ordinal");
        }
        
        public async Task SendTGNL(TrainModel trainModel)
        {
            ConsistList consistList = new ConsistList(trainModel);
            await PostToServer("Train", consistList);
        }
    }
}
