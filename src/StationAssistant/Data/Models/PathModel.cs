using StationAssistant.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StationAssistant.Data.Models
{
    public class PathModel
    {
        public int Id { get; set; }
        public string Area { get; set; }
        public string PathNum { get; set; }
        public bool Sort { get; set; }
        public short Occupation { get; set; }
        public short Length { get; set; }
        public string Marks { get; set; }
        public short EmptyVagonsCount { get; set; }

        public IGrouping<string, int> vagonKindCount { get; set; }
    }
}
