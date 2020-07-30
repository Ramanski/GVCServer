using GVCServer.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GVCServer.Data
{
    public interface IGuideRepository
    {
        public Task<List<string[]>> GetPlanFormStations(string sourceStation, string[] destinationStations);

        public Task<List<Schedule>> GetSchedule(string station);

        public Task<List<Pfclaim>> GetPlanFormClaims(string sourceStation);

        public Task<List<VagonKind>> GetVagonKinds();

        public Task<List<TrainKind>> GetTrainKinds();
    }
}
