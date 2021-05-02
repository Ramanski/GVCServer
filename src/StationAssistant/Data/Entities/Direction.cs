using System;
using System.Collections.Generic;

namespace StationAssistant.Data.Entities
{
    public partial class Direction
    {
        public string StationDestination { get; set; }
        public string PreferenceAreaArrival { get; set; }
        public string Track { get; set; }
        public byte ReqLength { get; set; }
        public short ReqWeight { get; set; }
        public byte MaxLength { get; set; }
        public short MaxWeight { get; set; }
        public short DirectionId { get; set; }
    }
}
