using System;
using System.Collections.Generic;

namespace StationAssistant.Data.Entities
{
    public partial class Vagon
    {
        public string Num { get; set; }
        public DateTime DateOper { get; set; }
        public string TrainIndex { get; set; }
        public string PlanForm { get; set; }
        public string Destination { get; set; }
        public byte? SequenceNum { get; set; }
        public string Ksob { get; set; }
        public short Tvag { get; set; }
        public short Kind { get; set; }
        public short? WeightNetto { get; set; }
        public string Mark { get; set; }
        public int? PathId { get; set; }
        public byte? VagonKindId { get; set; }

        public virtual Path Path { get; set; }
        public virtual Station PlanFormNavigation { get; set; }
        public virtual Train TrainIndexNavigation { get; set; }
        public virtual VagonKind VagonKind { get; set; }
    }
}
