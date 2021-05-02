using AutoMapper;
using GVCServer.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GVCServer.Models
{
    public class TrainSummary
    {
        public string TrainNum { get; set; }
        public string Index { get; set; }
        public short Length { get; set; }
        public short WeightBrutto { get; set; }
        public string Dislocation { get; set; }
        public string LastOperation { get; set; }
        public string DateOper { get; set; }
    }

    public class TrainList
    {
        public string TrainNum { get; set; }
        public string Index { get; set; }
        public DateTime FormTime { get; set; }
        public short Length { get; set; }
        public short WeightBrutto { get; set; }
        public string Oversize { get; set; }

        public List<WagonModel> Vagons { get; set; }
    }

}
