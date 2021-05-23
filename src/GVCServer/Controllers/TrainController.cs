using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using GVCServer.Data.Entities;
using GVCServer.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelsLibrary;
using ModelsLibrary.Codes;

namespace GVCServer.Controllers
{
    [ApiController]
    [Authorize]
    [Route("trains")]
    public class TrainController : ControllerBase
    {
        private readonly TrainRepository _trainRepository;
        private readonly WagonOperationsService wagonOperationsService;
        private string station { get; set; }
        private readonly ILogger<TrainController> _logger;

        public TrainController(ILogger<TrainController> logger, 
        TrainRepository trainRepository, 
        WagonOperationsService wagonOperationsService)
        {
            _logger = logger;
            _trainRepository = trainRepository;
            this.wagonOperationsService = wagonOperationsService;
        }

        [HttpGet("coming")]
        public async Task<ActionResult<TrainModel[]>> Get()
        {
            station = User?.Claims.Where(cl => cl.Type == ClaimTypes.Locality).FirstOrDefault()?.Value;
            TrainModel[] comingTrains = await _trainRepository.GetComingTrainsAsync(station);
            return (comingTrains.Length == 0) ? NoContent() : comingTrains;
        }

        [HttpGet("{trainId}")]
        public async Task<ActionResult<TrainModel>> GetTrainInfo(Guid trainId)
        {
            TrainModel trainModel = await _trainRepository.GetActualTrainAsync(trainId);
            return (trainModel == null) ? NoContent() : trainModel;
        }

        [HttpPost]
        public async Task<ActionResult<TrainModel>> CreateTrain([FromBody] ConsistList consistList, 
                                                                [FromServices] TrainOperationsService trainOperationsService,
                                                                [FromServices] WagonOperationsService wagonOperationsService)
        {
            station = User?.Claims.Where(cl => cl.Type == ClaimTypes.Locality).FirstOrDefault()?.Value;
            var createdTrain = await _trainRepository.AddTrainAsync(consistList.TrainModel, station);
            await wagonOperationsService.AttachToTrain(createdTrain, consistList.TrainModel.Wagons, consistList.DatOper, station);
            await trainOperationsService.CreateTrainAsync(createdTrain.Id, consistList.DatOper, station);
            return createdTrain;
        }

        [HttpDelete]
        public async Task<IActionResult> CancelTrainCreation(ConsistList cancelMsg)
        {
            station = User?.Claims.Where(cl => cl.Type == ClaimTypes.Locality).FirstOrDefault()?.Value;
            await _trainRepository.CancelTrainCreation(cancelMsg.TrainModel.Id, station);
            return Ok(); 
        }

        [HttpPut]
        public async Task<ActionResult<TrainModel>> UpdateTrain(TrainModel trainModel)
        {
            await _trainRepository.UpdateTrainParams(trainModel);
            return Ok();
        }
    }
}
