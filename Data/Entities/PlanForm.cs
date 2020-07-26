using System;
using System.Collections.Generic;

namespace GVCServer.Data.Entities
{
    public partial class PlanForm
    {
        public int Id { get; set; }
        public string FormStation { get; set; }
        public int LowRange { get; set; }
        public int HighRange { get; set; }
        public string TargetStation { get; set; }
        public byte TrainKind { get; set; }

        public virtual Station FormStationNavigation { get; set; }
        public virtual TrainKind TrainKindNavigation { get; set; }
    }
}
