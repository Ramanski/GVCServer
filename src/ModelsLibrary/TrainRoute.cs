using System;

namespace ModelsLibrary
{
    public class TrainRoute
    {
        public short TrainNumber { get; set; }
        public string DepartureStation { get; set; }
        public DateTime DepartureTime { get; set; }
        public string ArrivalStation { get; set; }
        public DateTime ArrivalTime { get; set; }
        
    }
}