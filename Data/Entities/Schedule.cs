using System;
using System.Text.Json.Serialization;

namespace GVCServer.Data.Entities
{
    public partial class Schedule
    {
        [JsonIgnore]
        public int Id { get; set; }
        [JsonIgnore]
        public string Station { get; set; }
        public short TrainNum { get; set; }
        public short DirectionId { get; set; }
        public TimeSpan? ArrivalTime { get; set; }
        public TimeSpan? DepartureTime { get; set; }

        public virtual Direction Direction { get; set; }
        public virtual Station StationNavigation { get; set; }
    }
}
