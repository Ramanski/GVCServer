using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using StationAssistant.Data;
using StationAssistant.Data.Entities;
using StationAssistant.Services;
using StationAssistant.Shared;
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

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages()
                    .AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.IgnoreNullValues = true;
                    });
            services.AddServerSideBlazor();
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

            services.AddScoped<INSIUpdateService, NsiUpdateService>();
            services.AddScoped<IGvcDataService, GvcDataService>();
            services.AddScoped<IStationDataService, StationDataService>();
            services.AddScoped<NotificationService>();
            services.AddAutoMapper(typeof(Startup));
            services.AddDbContext<StationStorageContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("StationStorage"))
                    );

            services.AddAuthentication( configureOptions =>
                    {
                        configureOptions.DefaultAuthenticateScheme = "AppCookie";
                        configureOptions.DefaultSignInScheme = "AppCookie";
                        configureOptions.DefaultChallengeScheme = "OAuth";
                    })
                    .AddCookie("AppCookie", conf =>
                    {
                        conf.Cookie.Name = "StationAssist.Cookie";
                        conf.LoginPath = "/login";
                    })
                    .AddOAuth("OAuth", conf =>
                    {
                        conf.AuthorizationEndpoint = Configuration["GVCServer:Authorizing"];
                        conf.TokenEndpoint = Configuration["GVCServer:Token"];
                        conf.CallbackPath = "/callback";
                        conf.ClientId = Configuration["Auth:Id"];
                        conf.ClientSecret = Configuration["Auth:Secret"];
                    });

            services.AddDbContext<UsersContext>(config =>
            {
                config.UseInMemoryDatabase("Memory");
            });

            // AddIdentity registers the services
            services.AddIdentity<IdentityUser, IdentityRole>(config =>
            {
                config.Password.RequiredLength = 4;
                config.Password.RequireDigit = false;
                config.Password.RequireNonAlphanumeric = false;
                config.Password.RequireUppercase = false;
                config.SignIn.RequireConfirmedEmail = true;
            })
                .AddEntityFrameworkStores<UsersContext>()
                .AddDefaultTokenProviders();

            services.AddAuthenticationCore();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IHttpService, HttpService>();
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MzM1ODEwQDMxMzgyZTMzMmUzMG9laG1XbUJkcnI0OHZpYTdicFdQVDdYK2ZTMzBXRDE5aDlhcnhYSTF0Y2c9");

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
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
