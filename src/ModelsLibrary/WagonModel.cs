

namespace ModelsLibrary
{
    public class WagonModel
    {
        public byte SequenceNum { get; set; }
        public string Num { get; set; }
        public string Ksob { get; set; }
        public short Kind { get; set; }
        public short Tvag { get; set; }
        public string Destination { get; set; }
        public short WeightNetto { get; set; }
        public byte Mark { get; set; }

        public TrainModel Train {get; set;}
    }
}
