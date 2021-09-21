using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GVCServer.Repositories;
using Microsoft.AspNetCore.Authorization;
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

        private string station { get; set; }

        public OperationsController(TrainRepository trainRepository,
                                    WagonOperationsService wagonOperationsService,
                                    TrainOperationsService trainOperationsService)
        {
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

        [HttpDelete]
        public async Task<ActionResult> CancelMovingOperation(MovingMsg movingMsg)
        {
            station = User?.Claims.Where(cl => cl.Type == ClaimTypes.Locality).FirstOrDefault()?.Value;
            var train = await _trainRepository.FindTrain(Guid.Parse(movingMsg.TrainId));
            
            if (train == null) 
                return NotFound(movingMsg.TrainId);

            await trainOperationsService.DeleteTrainOperation(train.Uid, station, movingMsg.Code);

            return Ok();
        }
    }
}
