﻿using GVCServer.Models;
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

        public Task<string> AddTrainAsync(Trains train, string station)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Appendix(string index, string station)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Arrival(string index, string station)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteTrain(string index)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Departure(string index, string station)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Disbanding(string index, string station)
        {
            throw new NotImplementedException();
        }

        public Task<Trains[]> GetComingTrainsAsync(string station, bool detailed)
        {
            throw new NotImplementedException();
        }

        public Task<Trains> GetTrainAsync(string index)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Proceed(string index, string station)
        {
            throw new NotImplementedException();
        }

        public Task<bool> PutTrainAsync(string index, List<Cars> cars, string station)
        {
            throw new NotImplementedException();
        }
    }
}
