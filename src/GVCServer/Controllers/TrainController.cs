using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GVCServer.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModelsLibrary;

namespace GVCServer.Controllers
{
    [ApiController]
    [Authorize]
    [Route("trains")]
    public class TrainController : ControllerBase
    {
        private readonly TrainRepository _trainRepository;
        private string Station { get; set; }

        public TrainController(TrainRepository trainRepository)
        {
            _trainRepository = trainRepository;
        }

        [HttpGet("coming")]
        public async Task<ActionResult<TrainModel[]>> Get()
        {
            Station = User?.Claims.Where(cl => cl.Type == ClaimTypes.Locality).FirstOrDefault()?.Value;
            TrainModel[] comingTrains = await _trainRepository.GetComingTrainsAsync(Station);
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
            Station = User?.Claims.Where(cl => cl.Type == ClaimTypes.Locality).FirstOrDefault()?.Value;
            var createdTrain = await _trainRepository.AddTrainAsync(consistList.TrainModel, Station);
            await wagonOperationsService.AttachToTrain(createdTrain, consistList.TrainModel.Wagons, consistList.DatOper, Station);
            await trainOperationsService.CreateTrainAsync(createdTrain.Id, consistList.DatOper, Station);
            return createdTrain;
        }

        [HttpDelete]
        public async Task<IActionResult> CancelTrainCreation(ConsistList cancelMsg)
        {
            Station = User?.Claims.Where(cl => cl.Type == ClaimTypes.Locality).FirstOrDefault()?.Value;
            await _trainRepository.CancelTrainCreation(cancelMsg.TrainModel.Id, Station);
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
