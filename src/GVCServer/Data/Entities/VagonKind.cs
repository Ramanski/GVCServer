using System;
using System.Collections.Generic;

namespace GVCServer.Data.Entities
{
    public partial class VagonKind
    {
        public VagonKind()
        {
            Vagon = new HashSet<Vagon>();
        }

        public byte Id { get; set; }
        public string Mnemocode { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Vagon> Vagon { get; set; }
    }
}
