using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using GVCServer.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StationAssistant.Auth;
using StationAssistant.Data;
using StationAssistant.Data.Entities;
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
                        options.JsonSerializerOptions.Converters.Add(new TimeSpanJsonConverter());
                        options.JsonSerializerOptions.IgnoreNullValues = true;
                    });
            services.AddControllers();
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
            services.AddTransient(sp => new HttpClient { BaseAddress = new Uri($"{Configuration["IVCaddress"]}/{Configuration["StationCode"]}/") });
            services.AddScoped<INSIUpdateService, NsiUpdateService>();
            services.AddScoped<IGvcDataService, GvcDataService>();
            services.AddScoped<IAccountsRepository, AccountsRepository>();
            services.AddScoped<IStationDataService, StationDataService>();
            services.AddScoped<NotificationService>();
            services.AddAutoMapper(typeof(Startup));
            services.AddDbContext<StationStorageContext>(options =>
                     options.UseSqlServer(Configuration.GetConnectionString("StationStorage")));

            services.AddIdentity<IdentityUser, IdentityRole>(op =>
            {
                op.Password.RequireDigit = false;
                op.Password.RequireNonAlphanumeric = false;
                op.Password.RequireUppercase = false;
                op.Password.RequireLowercase = false;
                op.Password.RequiredLength = 1;
            })
                    .AddEntityFrameworkStores<StationStorageContext>()
                    .AddDefaultTokenProviders();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options => 
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                         ValidateIssuer = false,
                         ValidateAudience = false,
                         ValidateLifetime = true,
                         ValidateIssuerSigningKey = true,
                         IssuerSigningKey = new SymmetricSecurityKey(
                             Encoding.UTF8.GetBytes(Configuration["jwt:key"])),
                             ClockSkew = TimeSpan.Zero
                    });
            services.AddAuthenticationCore();
            services.AddScoped<JWTAuthenticationStateProvider>();
            services.AddScoped<AuthenticationStateProvider, JWTAuthenticationStateProvider>(
                provider => provider.GetRequiredService<JWTAuthenticationStateProvider>()
                );
            services.AddScoped<ILoginService, JWTAuthenticationStateProvider>(
                provider => provider.GetRequiredService<JWTAuthenticationStateProvider>()
                );
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mjk4MzI5QDMxMzgyZTMyMmUzMGxSRGpya1djRG9mYlJ4MThOaUJxc0l2aktUNGs4M3NkSEtkV1kzSVE1U2M9");

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
                endpoints.MapControllers();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
