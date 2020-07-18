using AutoMapper;
using GVCServer.Data.Entities;
using GVCServer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GVCServer.Data
{
    public class TrainRepository : ITrainRepository
    {
        private readonly IVCStorageContext _context;
        private readonly IMapper _imapper;

        public TrainRepository(IVCStorageContext context, IMapper imapper)
        {
            _context = context;
            _imapper = imapper;
        }

        public Task<string> AddTrainAsync(Train train, string station)
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

        public async Task<TrainSummary[]> GetComingTrainsAsync(string station)
        {
            string targetNode = station.Substring(0, 4);
            Train[] trains = await _context.Train.Where(t => t.DestinationNode == targetNode)
                                                 .Include(t => t.OpTrain)
                                                 .Select(t => new Train { TrainNum = t.TrainNum,
                                                                          FormNode = t.FormNode,
                                                                          Ordinal = t.Ordinal,
                                                                          DestinationNode = t.DestinationNode,
                                                                          Length = t.Length,
                                                                          WeightBrutto = t.WeightBrutto,
                                                                          OpTrain = new List<OpTrain> { t.OpTrain.OrderByDescending(t => t.Msgid).FirstOrDefault() }
                                                 })
                                                 .ToArrayAsync();

            if (trains != null)
                return _imapper.Map<TrainSummary[]>(trains);
            else return null;
        }

        public Task<TrainList> GetTrainAsync(string index)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Proceed(string index, string station)
        {
            throw new NotImplementedException();
        }

        public Task<bool> PutTrainAsync(string index, List<Vagon> cars, string station)
        {
            throw new NotImplementedException();
        }
    }
}
