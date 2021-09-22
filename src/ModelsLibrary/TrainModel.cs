using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Serialization;

namespace ModelsLibrary
{
    public class TrainModel
    {
        public Guid Id {get; set;}
        public byte Kind { get; set; }
        public short Num { get; set; }
        public string FormStation { get; set; }
        public short Ordinal { get; set; }
        public string DestinationStation { get; set; }
        [JsonIgnore]
        public string Index { get => string.Format($"{FormStation.Substring(0,4)} {Ordinal.ToString().PadLeft(3,'0')} {DestinationStation.Substring(0,4)}"); }
        public string CodeOper { get; set; }
        public DateTime DateOper { get; set; }
        public string Dislocation { get; set; }
        public short Length { get; set; }
        public int WeightBrutto { get; set; }
        public IEnumerable<WagonModel> Wagons {get;set;}
        public PathModel Path { get; set; }

        public override string ToString()
        {
            return $@"Train {Index} ({Num}), kind: {Kind}, L/W: {Length}/{WeightBrutto},
             location:{Dislocation}, last: {CodeOper} on {DateOper.ToString("g", CultureInfo.CurrentCulture )}";
        }
    }
}
