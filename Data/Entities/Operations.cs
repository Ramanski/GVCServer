using System;
using System.Collections.Generic;

namespace GVCServer.Models
{
    public partial class Operations
    {
        public Operations()
        {
            OpVag = new HashSet<OpVag>();
        }

        public int Id { get; set; }
        public string Code { get; set; }
        public string Mnemonic { get; set; }
        public string Name { get; set; }

        public virtual ICollection<OpVag> OpVag { get; set; }
    }
}
