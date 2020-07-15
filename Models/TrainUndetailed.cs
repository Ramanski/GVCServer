using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GVCServer.Models
{
    public class TrainUndetailed
    {
        public string Np { get; set; }
        public string Nsos { get; set; }
        public string Index { get; set; }
        public string Ksnz { get; set; }
        public short Usdl { get; set; }
        public short Vesbr { get; set; }
        public string Ng { get; set; }
        public string LastOper { get; set; }
    }
}
