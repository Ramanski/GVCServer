using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GVCServer.Data.Entities;
using GVCServer.Repositories;
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
        }

        [Microsoft.AspNetCore.Authorization.Authorize]
        [HttpPost("pf")]
        public async Task<ActionResult<List<string[]>>> GetPFStations(string[] destination)
        {
            station = User.Claims.Where(cl => cl.Type == ClaimTypes.Locality).First().Value;
            if (destination.Length == 0)
                return BadRequest();
            return await _guideRepository.GetPlanFormStations(station, destination);
        }

        [HttpGet("train-Kind")]
        public async Task<ActionResult<byte>> GetTrainKind(string destination) 
        {
            station = User.Claims.Where(cl => cl.Type == ClaimTypes.Locality).First().Value;
            return await _guideRepository.GetTrainKind(station, destination);
        }

        [Microsoft.AspNetCore.Authorization.Authorize]
        [HttpGet("closest-departure")]
        public async Task<ActionResult<string[]>> GetClosestDeparture(int minsOffset, int direction, int kind) 
        {
            string station = User.Claims.Where(cl => cl.Type == ClaimTypes.Locality).First().Value;
            return await _guideRepository.GetClosestDeparture(station, kind, direction, minsOffset);
        }

        [HttpGet("operations")]
        public async Task<ActionResult<List<Operation>>> GetOperations() => await _guideRepository.GetOperations();

        [HttpGet("schedule")]
        public async Task<ActionResult<List<Schedule>>> GetSchedule()
        {
            station = User.Claims.Where(cl => cl.Type == ClaimTypes.Locality).First().Value;
            return await _guideRepository.GetSchedule(station);
        }    

        [HttpGet("pf/Claims")]
        public async Task<ActionResult<List<Pfclaim>>> GetPFclaims() 
        {
            station = User.Claims.Where(cl => cl.Type == ClaimTypes.Locality).First().Value;
            return await _guideRepository.GetPlanFormClaims(station);
        }

        [HttpGet("wagon/kinds")]
        public async Task<ActionResult<List<VagonKind>>> GetVagonKinds() => await _guideRepository.GetVagonKinds();

        [HttpGet("train/kinds")]
        public async Task<ActionResult<List<TrainKind>>> GetTrainKinds() => await _guideRepository.GetTrainKinds();
    }
}
