using System;
using System.Collections.Generic;

namespace GVCServer.Data.Entities
{
    public partial class OpTrain
    {
        public Guid Uid { get; set; }
        public Guid TrainId { get; set; }
        public string Kop { get; set; }
        public DateTime? Datop { get; set; }
        public DateTime? Msgid { get; set; }
        public string SourceStation { get; set; }

        public virtual Operation KopNavigation { get; set; }
        public virtual Station SourceStationNavigation { get; set; }
        public virtual Train Train { get; set; }
    }
}
