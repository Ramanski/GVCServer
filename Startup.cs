using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GVCServer.Data.Entities;
using GVCServer.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.SqlServer;
using GVCServer.Data;
using GVCServer.Helpers;
using GVCServer.Services;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

namespace GVCServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(options => options.Filters.Add(new HttpResponseExceptionFilter()))
                    .AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.IgnoreNullValues = true;
                        options.JsonSerializerOptions.Converters.Add(new TimeSpanJsonConverter());
                    });
            services.AddLocalization();

 services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            services.AddAutoMapper(typeof(Startup));
            services.AddScoped<ITrainRepository, TrainRepository>();
            services.AddScoped<IGuideRepository, GuideRepository>();
            services.AddDbContext<IVCStorageContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("IVCStorage")));
            services.AddScoped<IUserService, UserService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var supportedCultures = new[]{
    new CultureInfo("ru")
};
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("ru"),
                SupportedCultures = supportedCultures,
                FallBackToParentCultures = false
            });
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CreateSpecificCulture("ru");
            /*            if (env.IsDevelopment())
                        {
                            app.UseExceptionHandler("/error-local-development");
                        }*/

            //app.ConfigureExceptionHandler(logger);

            app.UseExceptionHandler("/error");

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseMiddleware<JwtMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
