using System;
using System.Collections.Generic;

namespace GVCServer.Models
{
    public partial class Cars
    {
        public Cars()
        {
            OpVag = new HashSet<OpVag>();
        }

        public string Nv { get; set; }
        public short? Pns { get; set; }
        public short? Ksob { get; set; }
        public short? Vesgr { get; set; }
        public string Stnz { get; set; }
        public byte? Otm { get; set; }
        public short Tvag { get; set; }

        public virtual ICollection<OpVag> OpVag { get; set; }
    }
}
