namespace ModelsLibrary.Codes
{

    public static class OperationCode{
        // Натурный лист на сформированный поезд
        public const string TrainComposition = "P0005";

        // Прицепка вагонов
        public const string AdditionVagons = "P0071";
        
        // Отцепка вагонов
        public const string DetachWagons = "P0072";
        
        // Корректировка вагонов
        public const string CorrectingComposition =  "P0073";

        // Изменение индекса поезда
        public const string TrainIndexUpdate = "P0074";

        // Прибытие поезда на станцию
        public const string TrainArrival = "P0001";
        
        // Отправление поезда со станции
        public const string TrainDeparture = "P0002";

        // Проследование поездом станции
        public const string TrainProceed = "P0003";

        // Расформирование поезда 
        public const string TrainDisbanding = "P0004";

        // Удаление сообщения
        public const string CancelMessage = "00000";
    }    
}