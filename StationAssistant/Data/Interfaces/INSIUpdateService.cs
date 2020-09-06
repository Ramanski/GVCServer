using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StationAssistant.Data
{
    interface INSIUpdateService
    {
            public Task<string> UpdateVagonKindsAsync();

            public Task<string> UpdateTrainKindsAsync();

            public Task<string> UpdatePFClaimsAsync();
    }
}
