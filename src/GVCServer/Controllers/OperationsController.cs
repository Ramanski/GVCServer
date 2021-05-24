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
using GVCServer.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModelsLibrary;
using ModelsLibrary.Codes;

namespace GVCServer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("{trainId}/operations")]
    public class OperationsController : ControllerBase
    {
        private readonly TrainRepository _trainRepository;
        private readonly WagonOperationsService wagonOperationsService;
        private readonly TrainOperationsService trainOperationsService;

        private string station {get ;set;}
        private readonly ILogger<TrainController> _logger;

        public OperationsController(ILogger<TrainController> logger, 
                                    TrainRepository trainRepository, 
                                    WagonOperationsService wagonOperationsService,
                                    TrainOperationsService trainOperationsService)
        {
            _logger = logger;
            _trainRepository = trainRepository;
            this.wagonOperationsService = wagonOperationsService;
            this.trainOperationsService = trainOperationsService;
        }

        [HttpPost]
        public async Task<ActionResult> AddMovingOperation(MovingMsg movingMsg)
        {
            station = User?.Claims.Where(cl => cl.Type == ClaimTypes.Locality).FirstOrDefault()?.Value;
            if (movingMsg.Code.Equals(OperationCode.TrainDisbanding))
            {
                var train = await _trainRepository.FindTrain(Guid.Parse(movingMsg.TrainId));
                await wagonOperationsService.DisbandWagons(train, station, movingMsg.DatOper);
            }
            await trainOperationsService.ProcessTrain(Guid.Parse(movingMsg.TrainId), station, movingMsg.DatOper, movingMsg.Code);

            return Ok();
        }
    }
}
