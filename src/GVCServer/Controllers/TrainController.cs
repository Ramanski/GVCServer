using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GVCServer.Data.Entities;
using GVCServer.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelsLibrary;
using ModelsLibrary.Codes;

namespace GVCServer.Controllers
{
    [ApiController]
    //[Authorize]
    [Route("trains")]
    public class TrainController : ControllerBase
    {
        private readonly TrainRepository _trainRepository;
        private readonly WagonOperationsService wagonOperationsService;

        private string station { get; set; }
        private readonly ILogger<TrainController> _logger;

        public TrainController(ILogger<TrainController> logger, 
        TrainRepository trainRepository, 
        WagonOperationsService wagonOperationsService)
        {
            _logger = logger;
            _trainRepository = trainRepository;
            this.wagonOperationsService = wagonOperationsService;
            station = "161306";//User?.Claims.Where(cl => cl.Type == ClaimTypes.Locality).FirstOrDefault()?.Value;
        }

        [HttpGet("coming")]
        public async Task<ActionResult<TrainModel[]>> Get()
        {
            TrainModel[] comingTrains = await _trainRepository.GetComingTrainsAsync(station);
            return (comingTrains.Length == 0) ? NoContent() : comingTrains;
        }

        [HttpGet("example")]
        public ActionResult<ConsistList> GetExample()
        {
            List<WagonModel> wagons = new List<WagonModel>(){ new WagonModel(){Num = "12345678", WeightNetto = 1234}};
            TrainModel model = new TrainModel(){
                                    Id = new Guid(), 
                                    DestinationStation = "161306", 
                                    Dislocation = "140007", 
                                    FormStation = "140007", 
                                    Kind = 20,
                                    Num = 1200,
                                    WeightBrutto = 3624,
                                    Wagons = wagons
                                    };
            
            return Ok(new ConsistList(OperationCode.TrainComposition, model, DateTime.Now));
        }

        [HttpGet("{trainId}")]
        public async Task<ActionResult<TrainModel>> GetTrainInfo(Guid trainId)
        {
            TrainModel trainModel = await _trainRepository.GetActualTrainAsync(trainId);
            return (trainModel == null) ? NoContent() : trainModel;
        }

        [HttpPost]
        public async Task<ActionResult<Train>> CreateTrain([FromBody] ConsistList consistList)
        {
            return await _trainRepository.AddTrainAsync(consistList.TrainModel, station);
        }

        [HttpDelete]
        public async Task<ActionResult> CancelTrainCreation(ConsistList cancelMsg, [FromServices] TrainOperationsService trainOperationsService)
        {
            await  trainOperationsService.DeleteLastTrainOperaion(cancelMsg.TrainModel.Index, cancelMsg.Code);
            return Ok();
        }

        [HttpPut]
        public async Task<ActionResult<TrainModel>> UpdateTrain(TrainModel trainModel)
        {
            await _trainRepository.UpdateTrainParams(trainModel);
            return Ok();
        }
    }
}
