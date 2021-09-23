using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ModelsLibrary
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
        public bool AnyTrain { get; set; }
        public int TrainLength { get; set; }
        public override string ToString()
        {
            return $"{ Area }№{ PathNum } ({ Marks?.Trim() })";
        }
    }
}
