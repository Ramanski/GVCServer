using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer.Data
{
    public class Apis
    {
        public static IEnumerable<ApiScope> GetScopes()
        {
            return new List<ApiScope>()
            {
                new ApiScope("gvc.read", "Read the data from GVC"),
                new ApiScope("gvc.write", "Add data to GVC"),
                new ApiScope("gvc.delete", "Cancel operation in GVC"),
            };
        }

        public static IEnumerable<ApiResource> GetResources()
        {
            return new List<ApiResource>()
            {
                new ApiResource("GVCServer", new string[]{ ClaimTypes.Role, ClaimTypes.Locality })
                {
                    Scopes = { "gvc.read", "gvc.write", "gvc.delete" }
                }
            };
        }
    }
}
