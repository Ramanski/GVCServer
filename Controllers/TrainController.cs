using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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


        public async Task<ActionResult<bool>> Post(MsgModel message, string station)
        {
            bool result = false;
            int msgCode = 0;

            msgCode = message.MsgId;

            try
            {
                switch (msgCode)
                {
                    case 2:
                        {
                            TrainList trainList = (TrainList) JsonSerializer.Deserialize<TrainList>(message.Body);
                            result = await _trainRepository.AddTrainAsync(trainList, station);
                            break;
                        }
                    case 9:
                        {

                            throw new NotImplementedException("Операция в разработке ...");
                        }
                    case 200:
                        {
                            throw new NotImplementedException("Операция в разработке ...");
                        }
                    case 201:
                        {
                            throw new NotImplementedException("Операция в разработке ...");
                        }
                    case 202:
                        {
                            throw new NotImplementedException("Операция в разработке ...");
                        }
                    case 203:
                        {
                            throw new NotImplementedException("Операция в разработке ...");
                        }
                    case 333:
                        {
                            throw new NotImplementedException("Операция в разработке ...");
                        }
                    default:
                        throw new ArgumentException($"Обработки сообщения c кодом {msgCode} не существует");
                }
            }
            catch (Exception e)
            {
                return new ObjectResult(e.Message);
            }

            return result;
        }

        [HttpPost("serialize")]
        public ActionResult<MsgModel> GetSerial(string station, TrainList trainList)
        {
            string json = JsonSerializer.Serialize<TrainList>(trainList);
            return new MsgModel { MsgId = 2, Parameter = 0, Body = json };
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
        public async Task<ActionResult<TrainList>> GetList(string station, string index)
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
