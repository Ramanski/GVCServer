using System;
using System.Collections.Generic;

namespace StationAssistant.Data.Entities
{
    public partial class Path
    {
        public Path()
        {
            Train = new HashSet<Train>();
            Vagon = new HashSet<Vagon>();
        }

        public int Id { get; set; }
        public string Area { get; set; }
        public string PathNum { get; set; }
        public short Occupation { get; set; }
        public short Length { get; set; }
        public short? Pfdirection { get; set; }
        public bool Arrival { get; set; }
        public bool Departure { get; set; }
        public bool Main { get; set; }
        public bool Sort { get; set; }
        public bool Passenger { get; set; }
        public bool Even { get; set; }
        public bool Odd { get; set; }
        public string Marks { get; set; }

        public virtual ICollection<Train> Train { get; set; }
        public virtual ICollection<Vagon> Vagon { get; set; }
    }
}
