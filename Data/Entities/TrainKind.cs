using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GVCServer.Data.Entities
{
    public partial class TrainKind
    {
        public TrainKind()
        {
            PlanForm = new HashSet<PlanForm>();
            Train = new HashSet<Train>();
        }

        public byte Code { get; set; }
        public string Mnemocode { get; set; }
        public string Name { get; set; }
        public short? TrainNumLow { get; set; }
        public short? TrainNumHigh { get; set; }

        public virtual ICollection<PlanForm> PlanForm { get; set; }
        public virtual ICollection<Train> Train { get; set; }
    }
}
