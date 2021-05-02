using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ModelsLibrary
{
    public class PFclaimsModel
    {
        public string StaDestination { get; set; }
        public byte ReqLength { get; set; }
        public short ReqWeight { get; set; }
        public byte MaxLength { get; set; }
        public short MaxWeight { get; set; }
    }
}
