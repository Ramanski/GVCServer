using System;
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
    [Route("{trainId}/wagons")]
    public class WagonsController : ControllerBase
    {
        private readonly ILogger<TrainController> logger;
        private readonly TrainRepository _trainRepository;
        private readonly WagonOperationsService wagonOperationsService;

        private string station { get; set; }

        public WagonsController(ILogger<TrainController> logger, TrainRepository trainRepository, WagonOperationsService wagonOperationsService)
        {
            this.logger = logger;
            this._trainRepository = trainRepository;
            this.wagonOperationsService = wagonOperationsService;
            station = User?.Claims.Where(cl => cl.Type == ClaimTypes.Locality).FirstOrDefault()?.Value;
        }

        [HttpPost]
        public async Task<ActionResult> AttachWagons(CorrectMsg correctMsg)
        {
            await wagonOperationsService.CorrectComposition(Guid.Parse(correctMsg.TrainId), correctMsg.WagonsList, correctMsg.DatOper, station);
            return Ok();
        }

        [HttpPut]
        public async Task<ActionResult> CorrectWagonsList(CorrectMsg correctMsg)
        {
            await wagonOperationsService.CorrectComposition(Guid.Parse(correctMsg.TrainId), correctMsg.WagonsList, correctMsg.DatOper, station);
            return Ok();
        }

        [HttpDelete]
        public async Task<ActionResult> DetachWagons(CorrectMsg correctMsg)
        {
            await wagonOperationsService.AddWagonOperations(Guid.Parse(correctMsg.TrainId), OperationCode.DetachWagons, correctMsg.WagonsList, correctMsg.DatOper, station);
            return Ok();
        }
    }
}