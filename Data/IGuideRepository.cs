using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GVCServer.Data
{
    public interface IGuideRepository
    {
        public Task<List<string[]>> GetPlanFormStations(string sourceStation, string[] destinationStations);
    }
}
