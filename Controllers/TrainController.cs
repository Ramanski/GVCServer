using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.Internal;
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

            try
            {
                switch (messageCode)
                {
                    case 2:
                        {
                            TrainList trainList = JsonSerializer.Deserialize<TrainList>(message.Body) as TrainList;
                            result = await _trainRepository.AddTrainAsync(trainList, station);
                            break;
                        }
                    case 9:
                        {
                            if (parameters.Length < 3)
                                throw new ArgumentNullException("Нe заданы все необходимые параметры в сообщении (индекс поезда, дата операции и код корректировки)");
                            string index = parameters[0];
                            DateTime timeOper = DateTime.Parse(parameters[1]);
                            string correctType = parameters[2];

                            switch (correctType)
                            {
                                // Корректировка сведений о вагонах
                                case "2":
                                // Прицепка вагонов
                                case "1":
                                    {
                                        List<VagonModel> newVagons = JsonSerializer.Deserialize<List<VagonModel>>(message.Body);
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
                                    throw new ArgumentException($"Обработки параметра операции \"{ parameters[1] }\" не существует");
                            }
                            break;
                        }
                    case 200:
                    case 201:
                    case 202:
                    case 203:
                        {
                            if (parameters.Length < 2)
                                throw new ArgumentNullException("Нe заданы все необходимые параметры в сообщении (индекс и дата операции)");
                            string index = parameters[0];
                            DateTime timeOper = DateTime.Parse(parameters[1]);

                            result = await _trainRepository.ProcessTrain(index, station, timeOper, messageCode.ToString());
                            break;
                        }
                    case 209:
                        {
                            string index = parameters[0];
                            short trainNum = short.Parse(parameters[1]);

                            result = await _trainRepository.UpdateTrainNum(index, trainNum);
                            break;
                        }
                    case 333:
                        {
                            if (parameters.Length < 2)
                                throw new ArgumentNullException("Нe заданы все необходимые параметры в сообщении (индекс и код отменяемого сообщения)");

                            string index = parameters[0];
                            string cancelMessageCode = parameters[1];

                            switch(cancelMessageCode)
                            {
                                case "200":
                                case "201":
                                case "202":
                                    {
                                        result = await _trainRepository.DeleteLastTrainOperaion(index, cancelMessageCode, false);
                                        break;
                                    }
                                case "2": 
                                case "203":
                                    {
                                        result = await _trainRepository.DeleteLastTrainOperaion(index, cancelMessageCode, true);                                     
                                        break;
                                    }
                                case "9":
                                    {
                                        if (message.Body == null)
                                        {
                                            result = await _trainRepository.DeleteLastTrainOperaion(index, cancelMessageCode,true);
                                        }
                                        else
                                        {
                                            string[] vagonNums = message.Body.Split(';');
                                                     vagonNums.ForAll(s => s.Trim());
                                            result = await _trainRepository.DeleteLastVagonOperaions(vagonNums, cancelMessageCode);
                                        }
                                        break;
                                    }                                
                            }
                        }
                        break;
                    default:
                        throw new ArgumentException($"Обработки сообщения c кодом {messageCode} не существует");
                }
            }
            catch (Exception e)
            {
                return new ObjectResult(e.Message);
            }

            return new StatusCodeResult(StatusCodes.Status200OK);
        }

        [HttpGet("Test/{msgCode}")]
        public ActionResult<MsgModel> GetTestMessages(string station, string msgCode)
        {
            int[] vagonNums = new int[] { 12345678, 22222222, 33333333, 44444444, 55555555, 66666666, 77777777, 88888888, 99999999, 11111111, 12343210, 63173519, 12958839, 19474005 };
            Random rnd = new Random();
            switch(msgCode)
            {
                case "2":
                    {
                        TrainList trainList = new TrainList
                        {
                            FormTime = DateTime.Now.AddHours(rnd.Next(-8, 0)),
                            Index = $"{station.Substring(0, 4)} {rnd.Next(1, 999).ToString().PadLeft(3, '0')} {rnd.Next(1000, 9999)}",
                            TrainNum = rnd.Next(1000, 4000).ToString(),
                            Oversize = "0000",
                            Vagons = new List<VagonModel>()
                        };
                        int i = rnd.Next(14);
                        while (i>0)
                        {
                            trainList.Vagons.Add(new VagonModel { Destination = "130100", Mark = (byte)rnd.Next(7), SequenceNum = (byte)i, Num = vagonNums[i].ToString(), WeightNetto = (short)rnd.Next(1000) });
                            i--;
                        }
                        trainList.Length = (short) trainList.Vagons.Count();
                        trainList.WeightBrutto = (short) (trainList.Vagons.Sum(v => v.WeightNetto) + rnd.Next(180, 320) * trainList.Length);

                        string json = JsonSerializer.Serialize(trainList);
                        return new MsgModel { Code = 2, Params = new string[] { station}, Body = json };
                    }
                default:
                    throw new Exception("Нету пока такого кода сообщения");
            }

        }

        [HttpGet("Arriving")]
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
                TrainList result = await _trainRepository.GetTrainListAsync(index);

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
