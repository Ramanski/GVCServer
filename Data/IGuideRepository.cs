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

        public Task<string[]> GetClosestDeparture(string station, int trainKind, int directionId, int minutesOffset);

        public Task<byte> GetTrainKind(string formStation, string destination);

        public Task<List<VagonKind>> GetVagonKinds();

        public Task<List<TrainKind>> GetTrainKinds();

        public Task<List<Operation>> GetOperations();
    }
}
