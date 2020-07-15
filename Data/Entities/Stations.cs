using System;
using System.Collections.Generic;

namespace GVCServer.Models
{
    public partial class Stations
    {
        public Stations()
        {
            OpVag = new HashSet<OpVag>();
        }

        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Mnemonic { get; set; }
        public bool? Department { get; set; }

        public virtual ICollection<OpVag> OpVag { get; set; }
    }
}
