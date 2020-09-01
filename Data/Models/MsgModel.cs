using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GVCServer.Models
{
    public class MsgModel
    {
        public int Code { get; set; }
        public string[] Params { get; set; }
        public string Body { get; set; }
    }
}
