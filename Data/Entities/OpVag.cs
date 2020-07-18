﻿using System;
using System.Collections.Generic;

namespace GVCServer.Data.Entities
{
    public partial class OpVag
    {
        public Guid Uid { get; set; }
        public DateTime? Msgid { get; set; }
        public string VagonNum { get; set; }
        public short NumRoute { get; set; }
        public string CodeOper { get; set; }
        public string Source { get; set; }
        public DateTime? DateOper { get; set; }
        public Guid? TrainId { get; set; }
        public string PlanForm { get; set; }
        public string Destination { get; set; }
        public byte? SequenceNum { get; set; }
        public short? WeightNetto { get; set; }
        public byte? Mark { get; set; }

        public virtual Operation CodeOperNavigation { get; set; }
        public virtual Station SourceNavigation { get; set; }
        public virtual Vagon Vagon { get; set; }
    }
}
