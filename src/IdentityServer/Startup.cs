using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using IdentityServer.Data;
using Microsoft.Extensions.Configuration;

namespace IdentityServer
{
    public class Startup
    {
        private readonly IConfiguration _conf;

        public Startup(IConfiguration conf)
        {
            _conf = conf;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            var connectionString = _conf.GetConnectionString("IdentityStorage");
            services.AddDbContext<UserContext>(config =>
            {
                config.UseSqlServer(connectionString);
            });

            services.AddIdentity<IdentityUser, IdentityRole>(config =>
            {
                config.Password.RequiredLength = 4;
                config.Password.RequireDigit = false;
                config.Password.RequireNonAlphanumeric = false;
                config.Password.RequireUppercase = false;
                config.SignIn.RequireConfirmedEmail = false;
            })
                .AddEntityFrameworkStores<UserContext>()
                .AddDefaultTokenProviders();

            var migrations = typeof(Startup).Assembly.GetName().Name;

            services.AddIdentityServer()
                    .AddAspNetIdentity<IdentityUser>()
                    .AddConfigurationStore(options =>
                    {
                        options.ConfigureDbContext = b => b.UseSqlServer(connectionString,
                            sql => sql.MigrationsAssembly(migrations));
                    })
                    .AddOperationalStore(options =>
                    {
                        options.ConfigureDbContext = b => b.UseSqlServer(connectionString,
                            sql => sql.MigrationsAssembly(migrations));
                    })
                    .AddDeveloperSigningCredential();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseIdentityServer();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
