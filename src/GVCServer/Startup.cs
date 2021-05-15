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
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using GVCServer.Repositories;

namespace GVCServer
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
            services.AddControllers(options => 
                options.Filters.Add<HttpResponseExceptionFilter>())
                    .AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.IgnoreNullValues = true;
                    });

            services.AddAutoMapper(typeof(Startup));
            services.AddScoped<TrainRepository>();
            services.AddScoped<GuideRepository>();
            services.AddScoped<WagonOperationsService>();
            services.AddScoped<TrainOperationsService>();

            services.AddDbContext<IVCStorageContext>(options => {
                    options.EnableDetailedErrors();
                    options.UseSqlServer(Configuration.GetConnectionString("IVCStorage"));
                    });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, conf =>
                    {
                        conf.Authority = Configuration["AppSettings:IdentityServer"];
                        conf.Audience = Configuration["AppSettings:ServerName"];
                    });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // if (env.IsDevelopment())
            // {
            //     app.UseDeveloperExceptionPage();
            // }
            //app.UseStaticFiles();

            app.UseHttpsRedirection();

            app.UseRouting();

            //app.UseAuthentication();

            //app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}