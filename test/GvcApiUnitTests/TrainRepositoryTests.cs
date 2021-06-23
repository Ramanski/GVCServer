using Xunit;
using GVCServer.Repositories;
using GVCServer.Data.Entities;
using Moq;
using AutoMapper;
using ModelsLibrary;
using ModelsLibrary.Codes;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace GvcApiUnitTests
{
    public class TrainRepositoryTests : IClassFixture<TrainRepositoryFixture>
    {
        TrainModel rawTrain;
        string sourceStation = "161306";
        public readonly Mock<IMapper> _mapperMock = new();
        public readonly Mock<ILogger<TrainRepository>> _loggerMock = new();
        private readonly IVCStorageContext contextMock;

        public TrainRepositoryTests(TrainRepositoryFixture trainRepositoryFixture)
        {
            this.contextMock = trainRepositoryFixture.context;
            rawTrain = new TrainModel(){ Id = System.Guid.Empty,
                            CodeOper = OperationCode.TrainComposition,
                            DestinationStation = "160002",
                            DateOper = DateTime.Now,
                            FormStation = sourceStation,
                            Length = 1,
                            Wagons = new List<WagonModel>() { new WagonModel() { Num = "00122239", SequenceNum = 1 } },
                            WeightBrutto = 100,
                            Kind = 20
                        };
        }

        [Fact]
        public async void AddTrain_DatabaseSavesTrainWithNewDislocation()
        {
            TrainRepository trainRepository = new TrainRepository(contextMock, _mapperMock.Object, _loggerMock.Object);

            var testTrain = await trainRepository.AddTrainAsync(rawTrain, sourceStation);

            _mapperMock.Verify(x => x.Map<TrainModel>(It.Is<Train>(t => 
                                                                !t.Uid.Equals(Guid.Empty) 
                                                                && t.Dislocation == sourceStation)));
            
            Assert.True(await contextMock.Train.CountAsync() >=1, "Не добавлен поезд в Train" );
            Assert.True(await contextMock.OpTrain.CountAsync() >=1, "Не добавлен поезд в OpTrain" );
            Assert.Equal(1, await contextMock.OpTrain.Where(o => o.LastOper).CountAsync());
        }

        [Fact]
        public async void CancelTrainCreation_DeletesDataInDatabase()
        {
            _mapperMock.Setup(m => m.Map<TrainModel>(It.IsAny<Train>())).Returns<Train>(t => new TrainModel(){ Id = t.Uid });
            TrainRepository trainRepository = new TrainRepository(contextMock, _mapperMock.Object, _loggerMock.Object);

            var testTrain = await trainRepository.AddTrainAsync(rawTrain, sourceStation);

            await trainRepository.CancelTrainCreation(testTrain.Id, sourceStation);
            Assert.Equal(0, await contextMock.Train.CountAsync());
            Assert.Equal(0, await contextMock.OpTrain.CountAsync());
        }


    }
}