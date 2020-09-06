using System;
using System.Collections.Generic;

namespace StationAssistant.Data.Entities
{
    public partial class TrainKind
    {
        public TrainKind()
        {
            Train = new HashSet<Train>();
        }

        public byte Code { get; set; }
        public string Mnemocode { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Train> Train { get; set; }
    }
}
