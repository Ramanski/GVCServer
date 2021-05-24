using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace IdentityServer.Data
{
    public class Clients
    {
        public static IEnumerable<Client> Get()
        {
            return new List<Client>
            {
                new Client
                {
                    AllowedGrantTypes = GrantTypes.Code,
                    ClientId = "client_id_mvc",
                    ClientSecrets = { new Secret("client_secret_mvc".Sha256()) },
                    AllowedScopes = {
                        IdentityServerConstants.StandardScopes.OpenId,
                        //IdentityServerConstants.StandardScopes.Profile
                        "gvc.read",
                        "gvc.write",
                        "gvc.delete",
                        "user.read"
                    },
                    RedirectUris = { "https://localhost:44357/signin-oidc" },
                    AllowOfflineAccess = true,
                },

                new Client
                {
                    AllowedGrantTypes = GrantTypes.Code,
                    ClientId = "station_assistant_161306",
                    ClientSecrets = { new Secret( "MxMzgyZTMzMmUzMG9laG1XbUJkcnI0OHZpDE#F3t(@K2ZTMzBXRDE5aDlhcnhYSTF".Sha256()) },
                    AllowAccessTokensViaBrowser = true,
                    AllowedScopes = {
                        IdentityServerConstants.StandardScopes.OpenId,
                        "gvc.read",
                        "gvc.write",
                        "gvc.delete",
                        "user.read"
                    },
                    UserSsoLifetime = null,
                    PostLogoutRedirectUris = { "https://localhost:5001/index" },
                    RedirectUris = { "https://localhost:5001/signin-oidc" },
                    AllowOfflineAccess = true,
                    AccessTokenLifetime = 3600,
                },

                new Client
                {
                    ClientId = "js",
                    ClientName = "JavaScript Client",
                    AllowedGrantTypes = GrantTypes.Code,
                    RequireClientSecret = false,
                    RedirectUris =           { "https://localhost:5001/callback.html" },
                    AllowedCorsOrigins =     { "https://localhost:5001" },
                    PostLogoutRedirectUris = { "https://localhost:5001/index.html" },
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        "user.read"
                    }
                }
            };
        }
    }
}
