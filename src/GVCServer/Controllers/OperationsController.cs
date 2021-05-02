using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper.Internal;
using GVCServer.Data;
using GVCServer.Helpers;
using GVCServer.Models;
using GVCServer.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModelsLibrary;
using ModelsLibrary.Codes;

namespace GVCServer.Controllers
{
    [ApiController]
    [Route("{trainIndex}/operations")]
    public class OperationsController : ControllerBase
    {
        private readonly ITrainRepository _trainRepository;
        private readonly WagonOperationsService wagonOperationsService;
        private readonly TrainOperationsService trainOperationsService;

        private string station {get ;set;}
        private readonly ILogger<TrainController> _logger;

        public OperationsController(ILogger<TrainController> logger, 
                                    ITrainRepository trainRepository, 
                                    WagonOperationsService wagonOperationsService,
                                    TrainOperationsService trainOperationsService)
        {
            _logger = logger;
            _trainRepository = trainRepository;
            this.wagonOperationsService = wagonOperationsService;
            this.trainOperationsService = trainOperationsService;
            station = User?.Claims.Where(cl => cl.Type == ClaimTypes.Locality).FirstOrDefault()?.Value;
        }

        [HttpPost("move")]
        public async Task<ActionResult> AddMovingOperation(MovingMsg movingMsg)
        {
            var train = await _trainRepository.FindTrain(movingMsg.TrainIndex);
            if (movingMsg.Code.Equals(OperationCode.TrainDisbanding))
            {
                await wagonOperationsService.DisbandWagons(train, station, movingMsg.DatOper);
            }
            await trainOperationsService.ProcessTrain(train.Uid, station, movingMsg.DatOper, movingMsg.Code);

            return Ok();
        }
    }
}
