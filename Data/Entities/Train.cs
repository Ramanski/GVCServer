using System;
using System.Collections.Generic;

namespace GVCServer.Data.Entities
{
    public partial class Train
    {
        public Train()
        {
            OpTrain = new HashSet<OpTrain>();
        }

        public Guid Uid { get; set; }
        public string SourceStation { get; set; }
        public string TrainNum { get; set; }
        public string FormNode { get; set; }
        public byte Ordinal { get; set; }
        public string DestinationNode { get; set; }
        public bool SequenceSign { get; set; }
        public DateTime? FormTime { get; set; }
        public short Length { get; set; }
        public short WeightBrutto { get; set; }
        public string Oversize { get; set; }

        public virtual ICollection<OpTrain> OpTrain { get; set; }
    }
}
