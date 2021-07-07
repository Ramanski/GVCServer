using System;
using Xunit;
using StationAssistant.Services;
using StationAssistant.Data.Entities;
using Moq;
using System.Collections.Generic;
using ModelsLibrary;

namespace StationAssistantTests
{
    public class GvcDataServiceTests
    {
        public readonly Mock<IHttpService> _httpServiceMock = new();
        public readonly Mock<StationStorageContext> _contextMock = new();
        private readonly GvcDataService gvcDataService;

        public GvcDataServiceTests()
        {
            gvcDataService = new(_contextMock.Object, _httpServiceMock.Object);
            _contextMock.Setup(s => s.Station.Find(It.IsAny<string>())).Returns(new Station());
        }

        [Fact]
        public async void GetArrivingTrains_ExceptionWhenNoComingTrains()
        {
            _httpServiceMock.Setup(s => s.Get<List<TrainModel>>("trains/coming").Result).Returns(new List<TrainModel>());

            await Assert.ThrowsAsync<RailProcessException>(() => gvcDataService.GetArrivingTrains());
        }

        [Theory]
        [InlineData("")]
        [InlineData("123456")]
        public async void GetArrivingTrains_DislocationMnemonicHasNoNulls(string station)
        {
            _httpServiceMock.Setup(s => s.Get<List<TrainModel>>("trains/coming").Result)
                            .Returns(new List<TrainModel>() { new TrainModel() { Dislocation = station } });

            var comingTrains = await gvcDataService.GetArrivingTrains();

            Assert.True(comingTrains.TrueForAll(t => !string.IsNullOrEmpty(t.Dislocation)));

            await Assert.ThrowsAsync<RailProcessException>(() => gvcDataService.GetArrivingTrains());
        }
    }
}
