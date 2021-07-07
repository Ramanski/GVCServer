using System;
using System.Collections.Generic;
using ModelsLibrary;

namespace GVCServer.Data.Entities
{
    public class Train
    {
        public Train()
        {
            OpTrain = new HashSet<OpTrain>();
            OpVag = new HashSet<OpVag>();
        }

        public static Train FromTrainModel(TrainModel trainModel)
        {
            return new Train(){
                            Uid = trainModel.Id,
                            TrainNum = trainModel.Num,
                            TrainKindId = trainModel.Kind,
                            FormStation = trainModel.FormStation,
                            Ordinal = trainModel.Ordinal,
                            DestinationStation = trainModel.DestinationStation,
                            FormTime = trainModel.DateOper,
                            Dislocation = trainModel.Dislocation,
                            Length = trainModel.Length,
                            WeightBrutto = trainModel.WeightBrutto
            };
        }

        public Guid Uid { get; set; }
        public short TrainNum { get; set; }
        public byte TrainKindId { get; set; }
        public string FormStation { get; set; }
        public short Ordinal { get; set; }
        public string DestinationStation { get; set; }
        public DateTime? FormTime { get; set; }
        
        // TODO: Delete
        public string Dislocation { get; set; }
        public short Length { get; set; }
        public int WeightBrutto { get; set; }
        public string Oversize { get; set; }

        public virtual TrainKind TrainKind { get; set; }
        public virtual ICollection<OpTrain> OpTrain { get; set; }
        public virtual ICollection<Schedule> Schedule { get; set; }
        public virtual ICollection<OpVag> OpVag { get; set; }
    }
}
