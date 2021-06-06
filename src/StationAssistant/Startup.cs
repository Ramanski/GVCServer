using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using StationAssistant.Data.Entities;
using StationAssistant.Services;
using Syncfusion.Blazor;

namespace StationAssistant
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddResponseCompression( opt =>
            {
                opt.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new []{"application/octet-stream"});
            });
            services.AddLocalization(opt => opt.ResourcesPath = "Resources");
            services.AddSyncfusionBlazor();
            services.AddSingleton(typeof(ISyncfusionStringLocalizer), typeof(SyncfusionLocalizer));
            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new List<CultureInfo>()
                {
                    new CultureInfo("ru")
                };
                options.DefaultRequestCulture = new RequestCulture("ru");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            }

            );
            services.AddTransient(sp => new HttpClient 
                { 
                    BaseAddress = new Uri(Configuration["GVCServer:BaseAddress"]) 
                });
            services.AddHttpClient();
            services.AddScoped<IHttpService, HttpService>();
            services.AddHttpContextAccessor();

            services.AddScoped<INSIUpdateService, NsiUpdateService>();
            services.AddScoped<IGvcDataService, GvcDataService>();
            services.AddScoped<IStationDataService, StationDataService>();
            services.AddScoped<NotificationService>();
            services.AddAutoMapper(typeof(Startup));
            services.AddDbContext<StationStorageContext>(options =>
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
                options.UseSqlServer(Configuration.GetConnectionString("StationStorage"));
            }
                    );

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
                        conf.Authority = Configuration["Auth:IdenServer"];
                        conf.ClientId = Configuration["Auth:Id"];
                        conf.ClientSecret = Configuration["Auth:Secret"];
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

            services.AddSingleton<BlazorServerAuthStateCache>();
            services.AddScoped<AuthenticationStateProvider, BlazorServerAuthState>();
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MzM1ODEwQDMxMzgyZTMzMmUzMG9laG1XbUJkcnI0OHZpYTdicFdQVDdYK2ZTMzBXRDE5aDlhcnhYSTF0Y2c9");

            app.UseResponseCompression();

            app.UseRequestLocalization(app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>().Value);
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapHub<TrainsHub>("/trainshub");
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
