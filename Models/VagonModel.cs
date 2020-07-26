using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace GVCServer.Models
{
    public class VagonModel
    {
        public byte SequenceNum { get; set; }
        public string VagonId { get; set; }
        [JsonIgnore]
        public short Ksob { get; set; }
        [JsonIgnore]
        public short Kind { get; set; }
        [JsonIgnore]
        public short Tvag { get; set; }
        public string Destination { get; set; }
        public short WeightNetto { get; set; }
        public byte Mark { get; set; }
    }
}
