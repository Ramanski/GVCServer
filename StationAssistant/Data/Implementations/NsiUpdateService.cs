using Microsoft.EntityFrameworkCore;
using ModelsLibrary;
using StationAssistant.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace StationAssistant.Data
{
    public class NsiUpdateService : INSIUpdateService
    {
        private HttpClient _client;
        private readonly IServiceProvider _provider;
        private readonly StationStorageContext _context;

        public NsiUpdateService(IServiceProvider provider, StationStorageContext context)
        {
            _provider = provider;
            _context = context;
            _client = (HttpClient)_provider.GetService(typeof(HttpClient));
        }

        public async Task<string> UpdateVagonKindsAsync()
        {
            List<VagonKind> newKinds;
            string result;

            var response = await _client.GetAsync("NSI/Vagon/Kinds");
            if (response.IsSuccessStatusCode)
            {
                newKinds = await response.Content.ReadFromJsonAsync<List<VagonKind>>();
                var oldKinds = _context.VagonKind;
                if (oldKinds.Any())
                {
                    _context.VagonKind.RemoveRange(oldKinds);
                    _context.SaveChanges();
                }
                _context.VagonKind.AddRange(newKinds);
                _context.SaveChanges();
                result = "Обновлено успешно.";
            }
            else
            {
                result = response.ReasonPhrase;
            }
            return result;
        }

        public async Task<string> UpdateTrainKindsAsync()
        {
            List<TrainKind> newKinds;
            string result;

            var response = await _client.GetAsync("NSI/Train/Kinds");
            if (response.IsSuccessStatusCode)
            {
                newKinds = await response.Content.ReadFromJsonAsync<List<TrainKind>>();
                var oldKinds = _context.TrainKind;
                if (oldKinds.Any())
                {
                    _context.TrainKind.RemoveRange(oldKinds);
                    _context.SaveChanges();
                }
                _context.TrainKind.AddRange(newKinds);
                _context.SaveChanges();
                result = "Обновлено успешно.";
            }
            else
            {
                result = response.ReasonPhrase;
            }
            return result;
        }


        public async Task<string> UpdatePFClaimsAsync()
        {
            List<PFclaimsModel> newClaims;
            string result;

            var response = await _client.GetAsync("NSI/PF/claims");
            if (response.IsSuccessStatusCode)
            {
                newClaims = await response.Content.ReadFromJsonAsync<List<PFclaimsModel>>();
                List<Direction> directions = await _context.Direction.ToListAsync();
                foreach(PFclaimsModel claim in newClaims)
                {
                    Direction direction = directions.Find(d => d.StationDestination.Equals(claim.StaDestination));
                    if(direction != null)
                    {
                        direction.MaxLength = claim.MaxLength;
                        direction.ReqLength = claim.ReqLength;
                        direction.MaxWeight = claim.MaxWeight;
                        direction.ReqWeight = claim.ReqWeight;
                    }
                }
                _context.SaveChanges();
                result = "Обновлено успешно.";
            }
            else
            {
                result = response.ReasonPhrase;
            }
            return result;
        }
    }
}
