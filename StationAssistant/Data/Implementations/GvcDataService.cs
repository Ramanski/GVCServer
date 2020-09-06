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

namespace StationAssistant.Data
{
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

            var response = await _client.GetAsync(requestUri);
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
            MsgModel msgArrive = new MsgModel { Code = 201, Params = new string[] { index, timeArrived.ToString() } };
            await PostToServer("Train", msgArrive);
        }

        public async Task<List<string[]>> GetNextDestinationStationsAsync(List<Vagon> vagons)
        {
            string[] destinations = vagons.Select(v => v.Destination).Distinct().ToArray();
            return await PostToServer<List<string[]>, string[]>("NSI/PF", destinations);
        }

        public async Task CancelOperation(string index, string msgCode)
        {
            MsgModel msgArrive = new MsgModel { Code = 333, Params = new string[] { index, msgCode} };
            await PostToServer("Train", msgArrive);
        }

        public async Task SendDisbanding(string index, DateTime timeDisbanded)
        {
            MsgModel msgDisband = new MsgModel { Code = 203, Params = new string[] { index, timeDisbanded.ToString() } };
            await PostToServer("Train", msgDisband);
        }

        public async Task SendDeparting(string index, DateTime timeDeparted)
        {
            MsgModel msgDepart = new MsgModel { Code = 200, Params = new string[] { index, timeDeparted.ToString() } };
            await PostToServer("Train", msgDepart);
        }

        public async Task<string[]> GetNearestScheduleRoute(int directionId, byte trainKind, int minutesOffset = 30)
        {
            return await GetFromServer<string[]>($"NSI/Closest-Departure?direction={directionId}&kind={trainKind}&minsOffset={minutesOffset}");
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
            MsgModel msgTGNL = new MsgModel { Code = 2 };
            msgTGNL.Body = JsonSerializer.Serialize(trainModel);
            await PostToServer("Train", msgTGNL);
        }
    }
}
