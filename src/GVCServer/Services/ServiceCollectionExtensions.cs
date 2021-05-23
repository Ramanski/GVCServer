using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Data;
using GVCServer.Data.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using GVCServer.Filters;

namespace GVCServer
{
public static class ServiceCollectionExtensions
    {
        public static IServiceCollection UseAllOfType<T>(this IServiceCollection serviceCollection, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            return serviceCollection.UseAllOfType<T>(new[] { typeof(ServiceCollectionExtensions).Assembly });
        }

        public static IServiceCollection UseAllOfType<T>(this IServiceCollection serviceCollection, Assembly[] assemblies, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            var typesFromAssemblies = assemblies.SelectMany(a => a.DefinedTypes.Where(x => x.IsClass && x.GetInterfaces().Contains(typeof(T))));
            foreach (var type in typesFromAssemblies)
                serviceCollection.Add(new ServiceDescriptor(type, type, lifetime));

            return serviceCollection;
        }

        public static IServiceCollection UseRegisterDbContext(this IServiceCollection serviceCollection, string connectionString, IsolationLevel level = IsolationLevel.ReadUncommitted)
        {
            //First, configure the SqlConnection and open it
            //This is done for every request/response
            serviceCollection.AddScoped<DbConnection>((serviceProvider) =>
            {
                var dbConnection = new SqlConnection(connectionString);
                dbConnection.Open();
                return dbConnection;
            });

            //Start a new transaction based on the SqlConnection
            //This is done for every request/response
            serviceCollection.AddScoped<DbTransaction>((serviceProvider) =>
            {
                var dbConnection = serviceProvider
                    .GetService<DbConnection>();

                return dbConnection.BeginTransaction(level);
            });

            //Create DbOptions for the DbContext, use the DbConnection
            //This is done for every request/response
            serviceCollection.AddScoped<DbContextOptions>((serviceProvider) =>
            {
                var dbConnection = serviceProvider.GetService<DbConnection>();
                return new DbContextOptionsBuilder<IVCStorageContext>()
                    .UseSqlServer(dbConnection)
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors()
                    .Options;
            });

            //Finally, create the DbContext, using the transaction
            //This is done for every request/response
            serviceCollection.AddScoped<IVCStorageContext>((serviceProvider) =>
            {
                var transaction = serviceProvider.GetService<DbTransaction>();
                var options = serviceProvider.GetService<DbContextOptions>();
                var context = new IVCStorageContext(options);
                context.Database.UseTransaction (transaction);
                return context;
            });

            return serviceCollection;
        }

        public static IServiceCollection UseOneTransactionPerHttpCall(this IServiceCollection serviceCollection)
        {
            //Manage the transaction at level of HTTP request/response
            //This is done for every request/response
            serviceCollection.AddScoped(typeof(TransactionFilter), typeof(TransactionFilter));

            serviceCollection
                .AddControllers(setup =>
                {
                    setup.Filters.AddService<TransactionFilter>(1);
                });

            return serviceCollection;
        }
    }
}