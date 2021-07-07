using System;
using System.Collections.Generic;

namespace GVCServer.Data.Entities
{
    public partial class Direction
    {
        public Direction()
        {
            Schedule = new HashSet<Schedule>();
        }

        public short Id { get; set; }
        public string Name { get; set; }
        public string DepartureStationId { get; set; }
        public string ArrivalStationId { get; set; }
        public virtual Station DepartureStation { get; set; }
        public virtual Station ArrivalStation { get; set; }

        public virtual ICollection<Schedule> Schedule { get; set; }
    }
}
