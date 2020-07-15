using System;
using System.Collections.Generic;

namespace GVCServer.Models
{
    public partial class Trains
    {
        public Guid Uid { get; set; }
        public string Ksos { get; set; }
        public string Np { get; set; }
        public string Ksfp { get; set; }
        public string Nsos { get; set; }
        public string Ksnz { get; set; }
        public bool Prss { get; set; }
        public DateTime DateForm { get; set; }
        public short Usdl { get; set; }
        public short Vesbr { get; set; }
        public string Ng { get; set; }

        public virtual OpVag OpVag { get; set; }
    }
}
