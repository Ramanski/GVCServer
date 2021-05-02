using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using GVCServer.Data;
using GVCServer.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModelsLibrary;
using ModelsLibrary.Codes;

namespace GVCServer.Controllers
{
    [ApiController]
    [Authorize]
    [Route("{trainIndex}/wagons")]
    public class WagonsController : ControllerBase
    {
        private readonly ILogger<TrainController> logger;
        private readonly ITrainRepository _trainRepository;
        private readonly WagonOperationsService wagonOperationsService;

        private string station { get; set; }

        public WagonsController(ILogger<TrainController> logger, ITrainRepository trainRepository, WagonOperationsService wagonOperationsService)
        {
            this.logger = logger;
            this._trainRepository = trainRepository;
            this.wagonOperationsService = wagonOperationsService;
            station = User?.Claims.Where(cl => cl.Type == ClaimTypes.Locality).FirstOrDefault()?.Value;
        }

        [HttpPost]
        public async Task<ActionResult> AttachWagons(CorrectMsg correctMsg)
        {
            await _trainRepository.CorrectVagons(correctMsg.TrainIndex, correctMsg.WagonsList, correctMsg.DatOper, station);
            return Ok();
        }

        [HttpPut]
        public async Task<ActionResult> CorrectWagonsList(CorrectMsg correctMsg)
        {
            await _trainRepository.CorrectVagons(correctMsg.TrainIndex, correctMsg.WagonsList, correctMsg.DatOper, station);
            return Ok();
        }

        [HttpDelete]
        public async Task<ActionResult> DetachWagons(CorrectMsg correctMsg)
        {
            var wagonNums = correctMsg.WagonsList.Select(w => w.Num).ToList();
            var trainId = _trainRepository.FindTrain(correctMsg.TrainIndex).Result.Uid;
            await wagonOperationsService.AddWagonOperations(trainId, OperationCode.DetachWagons, wagonNums, correctMsg.DatOper, station);
            return Ok();
        }
    }
}