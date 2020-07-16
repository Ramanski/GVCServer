using System;
using System.Collections.Generic;

namespace GVCServer.Data.Entities
{
    public partial class Vagon
    {
        public Vagon()
        {
            OpVag = new HashSet<OpVag>();
        }

        public string Nv { get; set; }
        public short? Ksob { get; set; }
        public short Tvag { get; set; }

        public virtual ICollection<OpVag> OpVag { get; set; }
    }
}
