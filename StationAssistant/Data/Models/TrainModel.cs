using StationAssistant.Data.Entities;
using StationAssistant.Data.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace StationAssistant.Models
{
    public class TrainModel
    {
        public string TrainNum { get; set; }
        public string Index { get; set; }
        public short Length { get; set; }
        public short WeightBrutto { get; set; }
        public string Dislocation { get; set; }
        public string LastOperation { get; set; }
        public string Oversize { get; set; }
        public DateTime DateOper { get; set; }

        public PathModel Path { get; set; }
        public List<VagonModel> Vagons { get; set; }
    }
}
