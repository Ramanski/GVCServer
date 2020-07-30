using System;
using System.Collections.Generic;

namespace GVCServer.Data.Entities
{
    public partial class Train
    {
        public Train()
        {
            OpTrain = new HashSet<OpTrain>();
            OpVag = new HashSet<OpVag>();
        }

        public Guid Uid { get; set; }
        public string TrainNum { get; set; }
        public byte? TrainKindId { get; set; }
        public string FormNode { get; set; }
        public short Ordinal { get; set; }
        public string DestinationNode { get; set; }
        public DateTime? FormTime { get; set; }
        public short Length { get; set; }
        public short WeightBrutto { get; set; }
        public string Oversize { get; set; }

        public virtual TrainKind TrainKind { get; set; }
        public virtual ICollection<OpTrain> OpTrain { get; set; }
        public virtual ICollection<OpVag> OpVag { get; set; }
    }
}
