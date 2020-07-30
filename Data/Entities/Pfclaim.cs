using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GVCServer.Data.Entities
{
    public partial class Pfclaim
    {
        [JsonIgnore]
        public int Id { get; set; }
        [JsonIgnore]
        public string StaForm { get; set; }
        public string StaDestination { get; set; }
        public byte ReqLength { get; set; }
        public short ReqWeight { get; set; }
        public byte MaxLength { get; set; }
        public short MaxWeight { get; set; }

        public virtual Station StaFormNavigation { get; set; }
    }
}
