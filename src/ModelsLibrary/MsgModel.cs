using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ModelsLibrary.Codes;

namespace ModelsLibrary
{
    public abstract class MsgModel
    {
        public abstract string Code { get; protected set; }
        public abstract DateTime DatOper { get; protected set; }
    }

    public class ConsistList : MsgModel
    {
		List<string> AllowedOperations = new List<string>(){ OperationCode.TrainIndexUpdate,
															 OperationCode.TrainComposition
															};
        public ConsistList(){}
        public ConsistList(string operCode, TrainModel trainModel, DateTime timeFormed)
        {
			if (!AllowedOperations.Exists(oper => operCode.Equals(oper)))
                throw new ArgumentOutOfRangeException("Недопустимый тип сообщения для данной операции");
            this.TrainModel = trainModel;
            DatOper = timeFormed;
        }
        public override string Code { get => OperationCode.TrainComposition; protected set{}}
        // Type = TrainModel
        [JsonInclude]
        public TrainModel TrainModel {get; private set;}
        [JsonInclude]
        public override DateTime DatOper { get; protected set;}
    }

    public class CorrectMsg : MsgModel
    {
        List<string> CorrectionOperations = new List<string>(){ OperationCode.DetachWagons, 
                                                                OperationCode.AdditionVagons, 
                                                                OperationCode.CorrectingComposition
																};
        public CorrectMsg(string correctionOper, List<WagonModel> wagonList, DateTime timeCorrected)
        {
            if (!CorrectionOperations.Exists(oper => correctionOper.Equals(oper)))
                throw new ArgumentOutOfRangeException("Недопустимый тип сообщения для данной операции");
            Code = correctionOper;
            WagonsList = wagonList;
            DatOper = timeCorrected;
        }
        [JsonInclude]
        public override string Code {get; protected set;}
        [JsonInclude]
        public List<WagonModel> WagonsList { get; private set;}
        [JsonInclude]
        public string TrainIndex { get; private set;}
        [JsonInclude]
        public override DateTime DatOper { get; protected set; }
    }

    public class MovingMsg : MsgModel
    {
        List<string> MovingOperations = new List<string>(){ OperationCode.TrainArrival,
                                                            OperationCode.TrainDeparture,
                                                            OperationCode.TrainProceed,
															OperationCode.TrainDisbanding															
															};

        public MovingMsg(string movingCode, string trainIndex, DateTime timeMoved)
        {
            if (!MovingOperations.Exists(oper => movingCode.Equals(oper)))
                throw new ArgumentOutOfRangeException("Недопустимый тип сообщения для данной операции");
            Code = movingCode;
            TrainIndex = trainIndex;
            DatOper = timeMoved;
        }
        [JsonInclude]
        public override string Code { get; protected set; }
        [JsonInclude]
        public string TrainIndex { get; private set;}
        [JsonInclude]
        public override DateTime DatOper { get; protected set;}
    }
}
