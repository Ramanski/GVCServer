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
using System.Linq;

namespace GvcApiUnitTests
{
    public class GuideRepositoryTests : IClassFixture<GuideRepositoryFixture>
    {
        private readonly string station = "161306";
        private readonly IVCStorageContext contextMock;

        public GuideRepositoryTests(GuideRepositoryFixture guideFixture)
        {
            this.contextMock = guideFixture.context;
        }

        [Fact]
        public async void GetClosestDeparture_ReturnsNextDateTime()
        {
            GuideRepository guideRepository = new(contextMock);
            var closestTrainRoute = await guideRepository.GetClosestDeparture(station:station, trainKind:10, directionId:1);
            Assert.Equal(DateTime.Today.AddDays(1), closestTrainRoute.DepartureTime.Date);
        } 
    }
}