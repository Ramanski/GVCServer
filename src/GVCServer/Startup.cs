using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using GVCServer.Repositories;
using Microsoft.Extensions.Hosting;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Http;

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
            services.AddControllers()
                .AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.IgnoreNullValues = true;
                    });
            services.AddProblemDetails(opts => {
                        opts.IncludeExceptionDetails = (context, ex) =>
                        {
                            var environment = context.RequestServices.GetRequiredService<IHostEnvironment>();
                            return environment.IsDevelopment();
                        };
                        opts.MapToStatusCode<ModelsLibrary.RailProcessException>(StatusCodes.Status400BadRequest);
                    });

            services.AddAutoMapper(typeof(Startup));
            services.AddScoped<TrainRepository>();
            services.AddScoped<GuideRepository>();
            services.AddScoped<WagonOperationsService>();
            services.AddScoped<TrainOperationsService>();

            services
                 .UseRegisterDbContext(Configuration.GetConnectionString("IVCStorage"))
                 .UseOneTransactionPerHttpCall();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, conf =>
                    {
                        conf.Authority = Configuration["AppSettings:IdentityServer"];
                        conf.Audience = Configuration["AppSettings:ServerName"];
                    });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseProblemDetails();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
