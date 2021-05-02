using System;
using System.Collections.Generic;

namespace GVCServer.Data.Entities
{
    public partial class Station
    {
        public Station()
        {
            OpTrain = new HashSet<OpTrain>();
            OpVag = new HashSet<OpVag>();
            Pfclaim = new HashSet<Pfclaim>();
            PlanForm = new HashSet<PlanForm>();
        }

        public string Code { get; set; }
        public string Name { get; set; }
        public string Mnemonic { get; set; }
        public bool? Department { get; set; }

        public virtual ICollection<OpTrain> OpTrain { get; set; }
        public virtual ICollection<OpVag> OpVag { get; set; }
        public virtual ICollection<Pfclaim> Pfclaim { get; set; }
        public virtual ICollection<PlanForm> PlanForm { get; set; }
    }
}
