using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GVCServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using ModelsLibrary.Codes;
using Xunit;

namespace GvcApiUnitTests
{
    public class GuideRepositoryFixture : IAsyncLifetime
    {
        public IVCStorageContext context;

        public async Task InitializeAsync()
        {
            DbContextOptionsBuilder<IVCStorageContext> dbContextOptions = new();
            dbContextOptions.UseSqlServer("Server=DESKTOP-4QIQNHM\\SQLEXPRESS;Database=testGVC;Trusted_Connection=True");
            context = new(dbContextOptions.Options);
            await context.Database.EnsureCreatedAsync();
            await Seed();
        }

        public async Task DisposeAsync()
        {
            await context.Database.EnsureDeletedAsync();
        }

        private Task<int> Seed()
        {
            context.TrainKind.Add(new TrainKind(){Code = 10, Mnemocode = "ТСТ", Name = "Участковый", TrainNumHigh = 2000, TrainNumLow = 1000});
            context.Schedule.Add(new Schedule(){Station = "161306", DirectionId = 1, TrainNum = 1200, ArrivalTime = TimeSpan.FromMinutes(10), DepartureTime = TimeSpan.FromMinutes(0)});
            context.Direction.Add(new Direction(){ DepartureStationId = "161306", Id = 1, ArrivalStationId = "160002", Name = "Полоцк-Витебск" });
            context.Station.AddRange(new List<Station>(){new Station(){Code = "160002", Name = "Витебск"}, 
                                                         new Station(){Code = "161306", Name = "Полоцк"}});
            return context.SaveChangesAsync();
        }
    }
}