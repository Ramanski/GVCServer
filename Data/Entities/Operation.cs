using System;
using System.Collections.Generic;

namespace GVCServer.Data.Entities
{
    public partial class Operation
    {
        public Operation()
        {
            OpTrain = new HashSet<OpTrain>();
            OpVag = new HashSet<OpVag>();
        }

        public int Id { get; set; }
        public string Code { get; set; }
        public string Mnemonic { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }

        public virtual ICollection<OpTrain> OpTrain { get; set; }
        public virtual ICollection<OpVag> OpVag { get; set; }
    }
}
