using System;
using System.Collections.Generic;

namespace StationAssistant.Data.Entities
{
    public partial class Train
    {
        public Train()
        {
            Vagon = new HashSet<Vagon>();
        }

        public string TrainIndex { get; set; }
        public string TrainNum { get; set; }
        public string FormStation { get; set; }
        public short Ordinal { get; set; }
        public string DestinationStation { get; set; }
        public DateTime? FormTime { get; set; }
        public short Length { get; set; }
        public int WeightBrutto { get; set; }
        public string Oversize { get; set; }
        public DateTime? DateOper { get; set; }
        public DateTime? ScheduleTime { get; set; }
        public int? PathId { get; set; }
        public byte TrainKindId { get; set; }

        public virtual Station DestinationStationNavigation { get; set; }
        public virtual Path Path { get; set; }
        public virtual ICollection<Vagon> Vagon { get; set; }
    }
}
