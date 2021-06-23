using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GVCServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using ModelsLibrary.Codes;
using Xunit;

namespace GvcApiUnitTests
{
    public class TrainRepositoryFixture : IAsyncLifetime
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
            context.VagonKind.Add(new VagonKind(){Id = 10, Mnemocode = "КР", Name = "Крытый"});
            context.TrainKind.Add(new TrainKind(){Code = 20, TrainNumLow = 1000, TrainNumHigh = 3000, Mnemocode = "УЧСТК", Name = "Участковый" });
            context.Operation.Add(new Operation(){ Code = OperationCode.TrainComposition, Message = "02" });
            context.Station.AddRange(new List<Station>(){new Station(){Code = "160002", Name = "Витебск"}, 
                                                         new Station(){Code = "161306", Name = "Полоцк"}});
            context.Vagon.Add(new Vagon(){ Id = "00122239", Kind = 10, Ksob = "БЧ", Tvag = 123 });
            return context.SaveChangesAsync();
        }
    }
}