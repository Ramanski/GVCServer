using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper.Internal;
using GVCServer.Data;
using GVCServer.Models;
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
            int messageCode = message.Code;
            string[] parameters = message.Params;

                switch (messageCode)
                {
                    case 2:
                        {
                            try
                            {
                                TrainModel TrainModel = JsonSerializer.Deserialize<TrainModel>(message.Body);
                                result = await _trainRepository.AddTrainAsync(TrainModel, station);
                            }
                            catch (Exception e)
                            {
                                throw new HttpResponseException()
                                {
                                    Status = (int)HttpStatusCode.BadRequest,
                                    Value = e.Message
                                };
                            }
                        break;
                        }
                    case 9:
                        {
                            if (parameters.Length < 3)
                                throw new HttpResponseException()
                                {
                                    Status = (int)HttpStatusCode.BadRequest,
                                    Value = "Нe заданы все необходимые параметры в сообщении (индекс поезда, дата операции и код корректировки)"
                                };

                            if(string.IsNullOrEmpty(message.Body))
                                throw new HttpResponseException()
                                {
                                    Status = (int)HttpStatusCode.BadRequest,
                                    Value = "Нe заданы вагоны для корректировки)"
                                };

                        string index = parameters[0];
                            DateTime timeOper = DateTime.Parse(parameters[1]);
                            string correctType = parameters[2];

                            try
                            {
                                switch (correctType)
                                {
                                    // Корректировка сведений о вагонах
                                    case "2":
                                    // Прицепка вагонов
                                    case "1":
                                        {
                                            List <VagonModel> newVagons = JsonSerializer.Deserialize<List<VagonModel>>(message.Body);
                                            result = await _trainRepository.CorrectVagons(index, newVagons, timeOper, station);
                                            break;
                                        }
                                    // Отцепка вагонов
                                    case "0":
                                        {
                                            string[] vagonsToDetach = message.Body.Split(';');
                                            result = await _trainRepository.DetachVagons(index, vagonsToDetach, timeOper, station);
                                            break;
                                        }
                                    default:
                                        throw new HttpResponseException()
                                        {
                                            Status = (int)HttpStatusCode.NotFound,
                                            Value = $"Обработки параметра операции \"{ parameters[1] }\" не существует"
                                        };
                                }
                            }
                            catch (Exception e)
                            {
                                throw new HttpResponseException()
                                {
                                    Status = (int)HttpStatusCode.BadRequest,
                                    Value = e.Message
                                };
                            }
                        break;
                        }
                    case 200:
                    case 201:
                    case 202:
                    case 203:
                        {
                            if (parameters.Length < 2)
                                throw new HttpResponseException()
                                {
                                    Status = (int)HttpStatusCode.BadRequest,
                                    Value = "Нe заданы все необходимые параметры в сообщении (индекс и дата операции)"
                                };
                            string index = parameters[0];
                        
                            try
                            {                          
                                DateTime timeOper = DateTime.Parse(parameters[1]);
                                result = await _trainRepository.ProcessTrain(index, station, timeOper, messageCode.ToString());
                            }
                            catch (Exception e)
                            {
                                throw new HttpResponseException()
                                {
                                    Status = (int)HttpStatusCode.BadRequest,
                                    Value = e.Message
                                };
                            }

                            break;
                        }
                    case 209:
                        {
                            string index = parameters[0];
                            short trainNum = short.Parse(parameters[1]);

                            try
                            {
                                result = await _trainRepository.UpdateTrainNum(index, trainNum);
                            }
                            catch (Exception e)
                            {
                                throw new HttpResponseException()
                                {
                                    Status = (int)HttpStatusCode.BadRequest,
                                    Value = e.Message
                                };
                            }
                        break;
                        }
                    case 333:
                        {
                            if (parameters.Length < 2)
                                throw new HttpResponseException() 
                                { 
                                    Status = (int)HttpStatusCode.BadRequest, 
                                    Value = "Нe заданы все необходимые параметры в сообщении (индекс и код отменяемого сообщения)" 
                                };

                            string index = parameters[0];
                            string cancelMessageCode = parameters[1];

                            switch (cancelMessageCode)
                            {
                                case "200":
                                case "201":
                                case "202":
                                    {
                                    try
                                    {
                                        result = await _trainRepository.DeleteLastTrainOperaion(index, cancelMessageCode, false);
                                    }
                                    catch(Exception e)
                                    {
                                        throw new HttpResponseException()
                                        {
                                            Status = (int)HttpStatusCode.BadRequest,
                                            Value = e.Message
                                        };
                                    }
                                        break;
                                    }
                                case "2":
                                case "203":
                                    {
                                        try
                                        {
                                            result = await _trainRepository.DeleteLastTrainOperaion(index, cancelMessageCode, true);
                                        }
                                        catch (Exception e)
                                        {
                                            throw new HttpResponseException()
                                            {
                                                Status = (int)HttpStatusCode.BadRequest,
                                                Value = e.Message
                                            };
                                        }
                                    break;
                                    }
                                case "9":
                                    {
                                        try
                                        {
                                            if (message.Body == null)
                                            {
                                                result = await _trainRepository.DeleteLastTrainOperaion(index, cancelMessageCode, true);
                                            }
                                            else
                                            {
                                                string[] vagonNums = message.Body.Split(';');
                                                vagonNums.ForAll(s => s.Trim());
                                                result = await _trainRepository.DeleteLastVagonOperaions(vagonNums, cancelMessageCode);
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            throw new HttpResponseException()
                                            {
                                                Status = (int)HttpStatusCode.BadRequest,
                                                Value = e.Message
                                            };
                                        }
                                    break;
                                    }
                            }
                        }
                        break;
                    default:
                        throw new HttpResponseException()
                        {
                            Status = (int)HttpStatusCode.NotFound,
                            Value = $"Обработки параметра операции \"{ parameters[1] }\" не существует"
                        };
                }

            if(result)
                return new StatusCodeResult(StatusCodes.Status200OK);
            else
                throw new HttpResponseException()
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Value = $"Неудача при сохранении операции в БД ГВЦ"
                };
        }

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

        [HttpGet("Next-Ordinal")]
        public async Task<ActionResult<short>> GetOrdinal(string station)
        {
            try
            {
                return await _trainRepository.GetNextOrdinal(station);
            }
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
                    Value = $"Не найден поезд по запрашиваемому индексу {index}"
                };
            }
            else
            {
                return result;
            }
            
        }
    }
}
