using System;
using System.Collections.Generic;

namespace GVCServer.Models
{
    public partial class OpVag
    {
        public Guid Uid { get; set; }
        public string Nv { get; set; }
        public short? Nrs { get; set; }
        public string Kop { get; set; }
        public DateTime? Datop { get; set; }
        public DateTime Msgid { get; set; }
        public Guid? Train { get; set; }
        public string Kso { get; set; }
        public string Snpf { get; set; }

        public virtual Operations KopNavigation { get; set; }
        public virtual Stations KsoNavigation { get; set; }
        public virtual Cars NvNavigation { get; set; }
        public virtual Trains U { get; set; }
    }
}
