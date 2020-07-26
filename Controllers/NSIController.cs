using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GVCServer.Data;
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

        [HttpGet("{PF}")]
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
    }
}
