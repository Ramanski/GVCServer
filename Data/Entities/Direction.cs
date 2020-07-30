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

        public virtual ICollection<Schedule> Schedule { get; set; }
    }
}
