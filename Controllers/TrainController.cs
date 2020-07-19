using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GVCServer.Data;
using GVCServer.Data.Entities;
using GVCServer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Logging;

namespace GVCServer.Controllers
{
    [ApiController]
    [Route("{station}/[controller]")]
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
        public async Task<ActionResult<TrainSummary[]>> Get(string station)
        {
            try
            {
                TrainSummary[] result = await _trainRepository.GetComingTrainsAsync(station);

                if (result.Length == 0)
                {
                    // TODO: Заменить номер станции на название
                    return new NotFoundObjectResult($"Нет поездов назначением на станцию {station}");
                }
                else
                {
                    return result;
                }
            }
            catch (Exception e)
            {
                return new ObjectResult(e.Message);
            }
        }


        [HttpGet("{index}")]
        public async Task<ActionResult<TrainList>> Get(string station, string index)
        {
            try
            {
                TrainList result = await _trainRepository.GetTrainAsync(index);

                if (result.Length == 0)
                {
                    return new NotFoundObjectResult($"Не найдено поездов по запрашиваемому индексу {index}");
                }
                else
                {
                    return result;
                }
            }
            catch (Exception e)
            {
                return new ObjectResult(e.Message);
            }
        }


    }
}
