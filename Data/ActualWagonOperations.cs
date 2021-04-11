using System;
using System.Text.Json.Serialization;

namespace GVCServer.Data.Entities
{
    public class ActualWagonOperations
    {
        [JsonIgnore]
        public Guid OperId {get; set;}
        public string WagonNum { get; set; }
        public Guid? TrainId { get; set; }
        public string TrainNum { get; set; }
        public string CodeOper { get; set; }
        public string Station { get; set; }
        public DateTime? DateOper { get; set; }
        public byte? SequenceNum { get; set; }
        public string PlanForm { get; set; }
        public string Destination { get; set; }
        public short? WeightNetto { get; set; }
        public string Mark { get; set; }
    }
}