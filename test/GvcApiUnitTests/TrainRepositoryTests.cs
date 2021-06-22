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

namespace GvcApiUnitTests
{
    public class TrainRepositoryTests : IClassFixture<TrainRepositoryFixture>
    {
        public readonly Mock<IMapper> _mapperMock = new();
        public readonly Mock<ILogger<TrainRepository>> _loggerMock = new();
        private readonly IVCStorageContext contextMock;

        public TrainRepositoryTests(TrainRepositoryFixture trainRepositoryFixture)
        {
            this.contextMock = trainRepositoryFixture.context;
        }

        [Fact]
        public async void AddTrain_AddingTrainToDatabaseWithNewDislocation()
        {
            TrainModel addedTrain;
            string sourceStation = "161306";
                TrainRepository trainRepository = new TrainRepository(contextMock, _mapperMock.Object, _loggerMock.Object);
              
            var rawTrain = new TrainModel(){ Id = System.Guid.Empty,
                                        CodeOper = OperationCode.TrainComposition,
                                        DestinationStation = "160002",
                                        DateOper = DateTime.Now,
                                        FormStation = sourceStation,
                                        Length = 1,
                                        Wagons = new List<WagonModel>() { new WagonModel() { Num = "00122239", SequenceNum = 1 } },
                                        WeightBrutto = 100,
                                        Kind = 20
                                    };

            addedTrain = await trainRepository.AddTrainAsync(rawTrain, sourceStation);

            _mapperMock.Verify(x => x.Map<TrainModel>(It.Is<Train>(t => 
                                                                !t.Uid.Equals(Guid.Empty) 
                                                                && t.Dislocation == sourceStation)));
        }

        [Fact]
        public async void AddTrain_AddsOperation()
        {
            Assert.True(await contextMock.Train.CountAsync() >=1, "Не добавлен поезд в Train" );
        }


    }
}