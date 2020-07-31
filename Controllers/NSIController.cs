using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GVCServer.Data;
using GVCServer.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GVCServer.Controllers
{
    [Route("{station}/[controller]")]
    [ApiController]
    public class NSIController : ControllerBase
    {
        private readonly IGuideRepository _guideRepository;

        public NSIController(IGuideRepository guideRepository)
        {
            _guideRepository = guideRepository;
        }

        [HttpGet("PF")]
        public async Task<ActionResult<List<string[]>>> GetPFStations(string station, string[] destination)
        {
            try
            {
                if (destination.Length == 0)
                    throw new ArgumentNullException("Не задано ни одной станции назначения");
                return await _guideRepository.GetPlanFormStations(station, destination);
            }
            catch (Exception e)
            {
                return new ObjectResult(e.Message);
            }
        }

        [HttpGet("Operations")]
        public async Task<ActionResult<List<Operation>>> GetOperations(string station)
        {
            try
            {
                return await _guideRepository.GetOperations();
            }
            catch (Exception e)
            {
                return new ObjectResult(e.Message);
            }
        }

        [HttpGet("Schedule")]
        public async Task<ActionResult<List<Schedule>>> GetSchedule(string station)
        {
            try
            {
                return await _guideRepository.GetSchedule(station);
            }
            catch (Exception e)
            {
                return new ObjectResult(e.Message);
            }
        }

        [HttpGet("PF/Claims")]
        public async Task<ActionResult<List<Pfclaim>>> GetPFclaims(string station)
        {
            try
            {
                return await _guideRepository.GetPlanFormClaims(station);
            }
            catch (Exception e)
            {
                return new ObjectResult(e.Message);
            }
        }

        [HttpGet("Vagon/Kinds")]
        public async Task<ActionResult<List<VagonKind>>> GetVagonKinds()
        {
            try
            {
                return await _guideRepository.GetVagonKinds();
            }
            catch (Exception e)
            {
                return new ObjectResult(e.Message);
            }
        }

        [HttpGet("Train/Kinds")]
        public async Task<ActionResult<List<TrainKind>>> GetTrainKinds()
        {
            try
            {
                return await _guideRepository.GetTrainKinds();
            }
            catch (Exception e)
            {
                return new ObjectResult(e.Message);
            }
        }
    }
}
