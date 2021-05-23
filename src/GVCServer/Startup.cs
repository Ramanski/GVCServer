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
                    });

            services.AddAutoMapper(typeof(Startup));
            services.AddScoped<TrainRepository>();
            services.AddScoped<GuideRepository>();
            services.AddScoped<WagonOperationsService>();
            services.AddScoped<TrainOperationsService>();

            services.AddDbContext<Data.Entities.IVCStorageContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("IVCStorage")));
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
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //app.UseStaticFiles();
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
