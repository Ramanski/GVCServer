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

        public string Id { get; set; }
        public short? Ksob { get; set; }
        public short Tvag { get; set; }
        public byte Kind { get; set; }

        public virtual VagonKind KindNavigation { get; set; }
        public virtual ICollection<OpVag> OpVag { get; set; }
    }
}
