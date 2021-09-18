using IdentityServer.Data;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelsLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using (var serviceScope = host.Services.CreateScope().ServiceProvider.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();
                if (!context.Clients.Any())
                {
                    foreach (var client in Clients.Get())
                    {
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.IdentityResources.Any())
                {
                    foreach (var resource in IdentityResources.Get())
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.ApiScopes.Any())
                {
                    foreach (var resource in Apis.GetScopes())
                    {
                        context.ApiScopes.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.ApiResources.Any())
                {
                    foreach (var resource in Apis.GetResources())
                    {
                        context.ApiResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                var manager = serviceScope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

                if (!manager.Users.Any())
                {
                    IdentityUser admin = new IdentityUser()
                    {
                        UserName = "admin",
                        Email = "test@test.com",
                        Id = "001",
                        EmailConfirmed = true
                    };

                    IEnumerable<Claim> claimsAdmin = new List<Claim>() {
                        new Claim(ClaimTypes.Locality, "161306"),
                        new Claim(ClaimTypes.Role, Role.Admin),
                        new Claim(ClaimTypes.Name, "Администратор")
                    };

                    IdentityUser polatsk = new IdentityUser()
                    {
                        UserName = "polatsk",
                        Email = "test@test.com",
                        Id = "002",
                        EmailConfirmed = true
                    };

                    IEnumerable<Claim> claimsPolatsk = new List<Claim>() {
                        new Claim(ClaimTypes.Locality, "161306"),
                        new Claim(ClaimTypes.Role, Role.DSP),
                        new Claim(ClaimTypes.Name, "ДСП Марковская")
                    };

                    IdentityUser vitebsk = new IdentityUser()
                    {
                        UserName = "vitebsk",
                        Email = "test@test.com",
                        Id = "003",
                        EmailConfirmed = true
                    };

                    IEnumerable<Claim> claimsVitebsk = new List<Claim>() {
                        new Claim(ClaimTypes.Locality, "160002"),
                        new Claim(ClaimTypes.Role, Role.DSC ),
                        new Claim(ClaimTypes.Name, "ДСЦ Сидоров")
                    };

                    manager.CreateAsync(admin, "password").GetAwaiter().GetResult();
                    manager.AddClaimsAsync(admin, claimsAdmin).GetAwaiter().GetResult();

                    manager.CreateAsync(vitebsk, "password").GetAwaiter().GetResult();
                    manager.AddClaimsAsync(vitebsk, claimsVitebsk).GetAwaiter().GetResult();

                    manager.CreateAsync(polatsk, "password").GetAwaiter().GetResult();
                    manager.AddClaimsAsync(polatsk, claimsPolatsk).GetAwaiter().GetResult();
                }
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
