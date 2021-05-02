using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer.Data
{
    public class IdentityResources
    {
        public static IEnumerable<IdentityResource> Get()
        {
            return new List<IdentityResource>()
            {
                new IdentityServer4.Models.IdentityResources.OpenId(),
                new IdentityServer4.Models.IdentityResources.Profile(),
                new IdentityResource
                {
                     Name = "user.read",
                     Description = "Claims needed to authorize user working on the station",
                     //Required = true,
                     UserClaims = {  
                         ClaimTypes.Locality,
                         ClaimTypes.Role, 
                         ClaimTypes.Name
                     }
                }
            };
        }
    }
}
