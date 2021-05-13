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
        public short TrainNum { get; set; }
        public byte? TrainKindId { get; set; }
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
