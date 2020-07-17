using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GVCServer.Data;
using GVCServer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GVCServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TrainController : ControllerBase
    {
        private readonly ITrainRepository _trainRepository;
        private readonly ILogger<TrainController> _logger;
        private readonly IMapper _mapper;

        public TrainController(ILogger<TrainController> logger, ITrainRepository trainRepository, IMapper mapper)
        {
            _logger = logger;
            _trainRepository = trainRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<TrainUndetailed[]>> Get(bool includeVagons = false)
        {
            try
            {
                var result = await _trainRepository.GetComingTrainsAsync("123456", includeVagons);

                //  return _imapper.Map<CampModel[]>(result);
                //return null;
            return new OkObjectResult ( new { Monika = "ATL2020", Name = "Atlanta Code Camp" });
            }
            catch (Exception)
            {
                return new StatusCodeResult(StatusCodes.Status501NotImplemented);
            }
        }

    }
}
