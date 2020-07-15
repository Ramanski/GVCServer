using GVCServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GVCServer.Data
{
    public class TrainRepository : ITrainRepository
    {
        private readonly IVCStorageContext _context;

        public TrainRepository(IVCStorageContext context)
        {
            _context = context;
        }

        public Task<string> AddTrain(Trains train, string station)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteTrain(string index)
        {
            throw new NotImplementedException();
        }


        public List<Trains> GetActualTrains(string station, bool detailed = false)
        {
            var actualTrains = _context.Trains.Where(t => t.Ksnz == station);
        }

        public Task<IEnumerable<Trains>> GetAllTrains()
        {
            throw new NotImplementedException();
        }

        public Task<Trains> GetTrainAsync(string index, bool detailed = false)
        {
            throw new NotImplementedException();
        }

        public Task<bool> PutTrain(string index, Trains train, string station)
        {
            throw new NotImplementedException();
        }
    }
}
