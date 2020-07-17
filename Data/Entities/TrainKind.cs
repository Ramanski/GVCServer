using System;
using System.Collections.Generic;

namespace GVCServer.Data.Entities
{
    public partial class TrainKind
    {
        public TrainKind()
        {
            PlanForm = new HashSet<PlanForm>();
        }

        public byte Code { get; set; }
        public string Mnemocode { get; set; }
        public string Name { get; set; }

        public virtual ICollection<PlanForm> PlanForm { get; set; }
    }
}
