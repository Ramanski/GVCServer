using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using GVCServer.Data;
using GVCServer.Data.Entities;
using GVCServer.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GVCServer.Controllers
{
    [Route("nsi")]
    [ApiController]
    public class NSIController : ControllerBase
    {
        private readonly GuideRepository _guideRepository;
        private string station { get; set; }

        public NSIController(GuideRepository guideRepository)
        {
            _guideRepository = guideRepository;
            station = User?.Claims.Where(cl => cl.Type == ClaimTypes.Locality).FirstOrDefault()?.Value;
        }

        [HttpPost("pf")]
        public async Task<ActionResult<List<string[]>>> GetPFStations(string[] destination)
        {
            if (destination.Length == 0)
                return BadRequest();
            return await _guideRepository.GetPlanFormStations(station, destination);
        }

        [HttpGet("train-Kind")]
        public async Task<ActionResult<byte>> GetTrainKind(string station, string destination) => await _guideRepository.GetTrainKind(station, destination);

        [HttpGet("closest-departure")]
        public async Task<ActionResult<string[]>> GetClosestDeparture(string station, int minsOffset, int direction, int kind) => await _guideRepository.GetClosestDeparture(station, kind, direction, minsOffset);

        [HttpGet("operations")]
        public async Task<ActionResult<List<Operation>>> GetOperations(string station) => await _guideRepository.GetOperations();

        [HttpGet("schedule")]
        public async Task<ActionResult<List<Schedule>>> GetSchedule(string station) => await _guideRepository.GetSchedule(station);

        [HttpGet("pf/Claims")]
        public async Task<ActionResult<List<Pfclaim>>> GetPFclaims(string station) => await _guideRepository.GetPlanFormClaims(station);

        [HttpGet("wagon/kinds")]
        public async Task<ActionResult<List<VagonKind>>> GetVagonKinds() => await _guideRepository.GetVagonKinds();

        [HttpGet("train/kinds")]
        public async Task<ActionResult<List<TrainKind>>> GetTrainKinds() => await _guideRepository.GetTrainKinds();
    }
}
