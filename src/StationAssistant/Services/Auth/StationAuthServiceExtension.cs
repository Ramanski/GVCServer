using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace StationAssistant
{
    public static class StationAuthServiceExtension
    {
        public static IServiceCollection AddStationAuthentication(this IServiceCollection services, IConfiguration authConfiguration)
        {
            services.AddAuthentication( configureOptions =>
                    {
                        configureOptions.DefaultScheme = "AppCookie";
                        configureOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                    })
                    .AddCookie("AppCookie", conf =>
                    {
                        conf.Cookie.Name = "StationAssist.Cookie";
                    })
                    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, conf =>
                    {
                        conf.Authority = authConfiguration["IdenServer"];
                        conf.ClientId = authConfiguration["Id"];
                        conf.ClientSecret = authConfiguration["Secret"];
                        conf.ResponseType = OpenIdConnectResponseType.Code;
                        conf.SaveTokens = true;
                        conf.GetClaimsFromUserInfoEndpoint = true;
                        conf.SignedOutCallbackPath = "/index";

                        conf.ClaimActions.MapUniqueJsonKey(ClaimTypes.Role, ClaimTypes.Role);
                        conf.ClaimActions.MapUniqueJsonKey(ClaimTypes.Name, ClaimTypes.Name);
                        
                        conf.Scope.Clear();
                        conf.Scope.Add(OpenIdConnectScope.OpenId);
                        conf.Scope.Add(OpenIdConnectScope.OfflineAccess);
                        conf.Scope.Add("gvc.read");
                        conf.Scope.Add("gvc.write");
                        conf.Scope.Add("gvc.delete");
                        conf.Scope.Add("user.read");

                        conf.Events = new OpenIdConnectEvents
                        {
                            // that event is called after the OIDC middleware received the auhorisation code,
                            // redeemed it for an access token and a refresh token,
                            // and validated the identity token
                            OnTokenValidated = x =>
                            {
                                // so that we don't issue a session cookie but one with a fixed expiration
                                x.Properties.IsPersistent = true;

                                // align expiration of the cookie with expiration of the
                                // access token
                                var accessToken = new JwtSecurityToken(x.TokenEndpointResponse.AccessToken);
                                x.Properties.ExpiresUtc = accessToken.ValidTo;

                                return Task.CompletedTask;
                            }
                        };
                    });
            return services;
        }
    }
}