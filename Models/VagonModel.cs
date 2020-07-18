using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GVCServer.Models
{
    public class VagonModel
    {
        public byte SequenceNum { get; set; }
        public string VagonNum { get; set; }
        public short Ksob { get; set; }
        public short Tvag { get; set; }
        public string Destination { get; set; }
        public short WeightNetto { get; set; }
        public byte Mark { get; set; }
    }
}
