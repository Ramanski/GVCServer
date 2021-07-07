using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using AutoMapper;
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
            services.AddScoped<IHttpService, HttpService>();
            services.AddHttpContextAccessor();

            services.AddScoped<IGvcDataService, GvcDataService>();
            services.AddScoped<IStationDataService, StationDataService>();
            services.AddScoped<NotificationService>();
            
            services.AddAutoMapper(typeof(Startup));
            services.AddDbContext<StationStorageContext>(options =>
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
                options.UseSqlServer(Configuration.GetConnectionString("StationStorage"));
            });

            services.AddStationAuthentication(Configuration.GetSection("Auth"));

            services.AddSingleton<BlazorServerAuthStateCache>();
            services.AddScoped<AuthenticationStateProvider, BlazorServerAuthState>();
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(Configuration["Syncfusion:LicenceKey"]);

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
