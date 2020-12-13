using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper.Internal;
using GVCServer.Data;
using GVCServer.Helpers;
using GVCServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ModelsLibrary;

namespace GVCServer.Controllers
{
    [ApiController]
    [Route("{station}/[controller]")]
    public class TrainController : ControllerBase
    {
        private readonly ITrainRepository _trainRepository;
        private readonly ILogger<TrainController> _logger;

        public TrainController(ILogger<TrainController> logger, ITrainRepository trainRepository)
        {
            _logger = logger;
            _trainRepository = trainRepository;
        }
        
        [HttpPost]
        public async Task<ActionResult<bool>> Post(MsgModel message, string station)
        {
            bool result = false;
            _logger.LogInformation("Got {messageCode} type message from {station}", message.Code, station);

            try
            {
                switch (message.Code)
                {
                    case 2:
                        {
                            ConsistList consistList;
                            TrainModel trainModel;

                            consistList = (ConsistList)message;
                            trainModel = JsonSerializer.Deserialize<TrainModel>(consistList.Body);
                            result = await _trainRepository.AddTrainAsync(trainModel, station);
                            break;
                        }
                    case 9:
                        {
                            CorrectMsg correctMsg;
                            List<VagonModel> vagons;

                            correctMsg = (CorrectMsg)message;
                            vagons = JsonSerializer.Deserialize<List<VagonModel>>(correctMsg.Body);

                            switch (correctMsg.Sign)
                            {
                                // Корректировка сведений о вагонах
                                case 2:
                                // Прицепка вагонов
                                case 1:
                                    {
                                        result = await _trainRepository.CorrectVagons(correctMsg.TrainIndex,
                                                                                      vagons,
                                                                                      correctMsg.DatOper,
                                                                                      station);
                                        break;
                                    }
                                // Отцепка вагонов
                                case 0:
                                    {
                                        // TODO: Format vagons as List in StationAssistant
                                        result = await _trainRepository.DetachVagons(correctMsg.TrainIndex,
                                                                                     vagons,
                                                                                     correctMsg.DatOper,
                                                                                     station);
                                        break;
                                    }
                                default:
                                    throw new ArgumentOutOfRangeException($"Обработки параметра операции \"{ correctMsg.Sign }\" не существует");
                            }
                            break;
                        }
                    case 200:
                    case 201:
                    case 202:
                    case 203:
                        {
                            MovingMsg movingMsg = (MovingMsg)message;
                            result = await _trainRepository.ProcessTrain(movingMsg.TrainIndex,
                                                                         station,
                                                                         movingMsg.DatOper,
                                                                         movingMsg.Code.ToString());
                            break;
                        }
                    case 209:
                        {
                            TrainNumMsg trainNumMsg = (TrainNumMsg)message;
                            result = await _trainRepository.UpdateTrainNum(trainNumMsg.TrainIndex, trainNumMsg.TrainNum);
                            break;
                        }
                    case 333:
                        {
                            CancelMsg cancelMsg = (CancelMsg)message;

                            if (message.Body == null)
                            {
                                result = await _trainRepository.DeleteLastTrainOperaion(cancelMsg.TrainIndex,
                                                                                        cancelMsg.TargetCode.ToString(),
                                                                                        true);
                            }
                            else
                            {
                                string[] vagonNums = cancelMsg.Body.Split(';');
                                vagonNums.ForAll(s => s.Trim());
                                result = await _trainRepository.DeleteLastVagonOperaions(vagonNums, cancelMsg.TargetCode.ToString());
                            }
                            break;
                        }
                    default:
                        throw new ArgumentOutOfRangeException($"Обработки сообщения с кодом \"{ message.Code }\" не существует");
                }
            }
            catch (InvalidCastException ex)
            {
                throw new HttpResponseException()
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Value = "Структура сообщения не соответствует указанному типу."
                };
            }
            catch (Exception e)
            {
                throw new HttpResponseException()
                {
                    Status = (int)HttpStatusCode.InternalServerError,
                    Value = "Ошибка сервера"
                };
            }

            if (result)
                return new StatusCodeResult(StatusCodes.Status200OK);
            else
                throw new HttpResponseException()
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Value = $"Неудача при проведении операции."
                };
        }

        [Authorize]
        [HttpGet("Arriving")]
        public async Task<ActionResult<TrainModel[]>> Get(string station)
        {
            TrainModel[] result;

            try
            {
                result = await _trainRepository.GetComingTrainsAsync(station);
            }
            catch (Exception e)
            {
                throw new HttpResponseException()
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Value = e.Message
                };
            }

            if (result.Length == 0)
            {
                // TODO: Заменить номер станции на название
                throw new HttpResponseException()
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Value = $"Нет поездов назначением на станцию {station}"
                };
            }
            else
            {
                return result;
            }
        }

        [Authorize]
        [HttpGet("Next-Ordinal")]
        public async Task<ActionResult<short>> GetOrdinal(string station)
        {
            try
            {
                return await _trainRepository.GetNextOrdinal(station);
            }
            // TODO: Заменить конкретным эксепшеном
            catch (Exception)
            {
                throw new HttpResponseException()
                {
                    Status = (int)HttpStatusCode.NotFound,
                    Value = $"Порядковый номер поезда для станции {station} неопределен"
                };
            }
        }

        [HttpGet("{index}")]
        public async Task<ActionResult<TrainModel>> GetList(string station, string index)
        {
            TrainModel result;

            try
            {
                result = await _trainRepository.GetTrainModelAsync(index);
            }
            // TODO: Заменить конкретным эксепшеном
            catch (Exception e)
            {
                throw new HttpResponseException()
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Value = e.Message
                };
            }

            if (result.Length == 0)
            {
                throw new HttpResponseException()
                {
                    Status = (int)HttpStatusCode.NotFound,
                    Value = $"По запрашиваемому индексу  {index} поездов не найдено"
                };
            }
            return result;            
        }
    }
}
