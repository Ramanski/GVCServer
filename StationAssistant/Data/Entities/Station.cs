using System;
using System.Collections.Generic;

namespace StationAssistant.Data.Entities
{
    public partial class Station
    {
        public Station()
        {
            Train = new HashSet<Train>();
            Vagon = new HashSet<Vagon>();
        }

        public string Code { get; set; }
        public string Name { get; set; }
        public string Mnemonic { get; set; }
        public bool? Department { get; set; }

        public virtual ICollection<Train> Train { get; set; }
        public virtual ICollection<Vagon> Vagon { get; set; }
    }
}
