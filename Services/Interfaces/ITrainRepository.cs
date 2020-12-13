using GVCServer.Data.Entities;
using ModelsLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GVCServer.Data
{
    public interface ITrainRepository
    {
        /// <summary>
        /// Получение сведений о поезде в объеме ТГНЛ
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        Task<TrainModel> GetTrainModelAsync(string index);

        /// <summary>
        /// Корректировка сведений о составе поезда (сообщение 09, прицепка - P0071, корректировка - P0073)
        /// </summary>
        /// <param name="index"></param>
        /// <param name="newList"></param>
        /// <param name="station"></param>
        /// <returns></returns>
        public Task<bool> CorrectVagons(string index, List<VagonModel> newList, DateTime timeOper, string station);

        /// <summary>
        /// Отцепка вагонов от состава поезда (сообщение 09, P0072)
        /// </summary>
        /// <param name="index"></param>
        /// <param name="vagonNums"></param>
        /// <param name="station"></param>
        /// <returns></returns>
        public Task<bool> DetachVagons(string index, List<VagonModel> vagonNums, DateTime timeOper, string station);

        /// <summary>
        /// Запись поезда из ТНГЛ (сообщение 02, P0005)
        /// </summary>
        /// <param name="TrainModel"></param>
        /// <param name="station"></param>
        /// <returns></returns>
        public Task<bool> AddTrainAsync(TrainModel TrainModel, string station);

        public Task<short> GetNextOrdinal(string formStation);

        public Task<bool> UpdateTrainNum(string index, short TrainNum);

        /// <summary>
        /// Получение общих сведений на прибывающие поезда
        /// </summary>
        /// <param name="station"></param>
        /// <returns></returns>
        Task<TrainModel[]> GetComingTrainsAsync(string station);

        /// <summary>
        /// Отмена последней операции с поездом (сообщение 333)
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Task<bool> DeleteLastTrainOperaion(string index, string messageCode, bool includeVagonOperations);

        /// <summary>
        /// Отмена последней операции с вагоном (вагонами) (сообщение 333)
        /// </summary>
        /// <param name="train"></param>
        /// <param name="vagonNum"></param>
        /// <returns></returns>
        public Task<bool> DeleteLastVagonOperaions(string index, string messageCode);
        public Task<bool> DeleteLastVagonOperaions(string[] vagonNum, string messageCode);

        /// <summary>
        /// Обработка операции следования поездом (P0001, P0002, P0003)
        /// </summary>
        /// <param name="index"></param>
        /// <param name="station"></param>
        /// <param name="timeOper"></param>
        /// <param name="codeOper"></param>
        /// <returns></returns>
        public Task<bool> ProcessTrain(string index, string station, DateTime timeOper, string messageCode);

        /// <summary>
        /// Оформление операции расформирования поезда (203, P0004)
        /// </summary>
        /// <param name="index">Индекс поезда</param>
        /// <param name="station">Станция совершения операции</param>
        /// <returns></returns>
        public Task<bool> DisbandVagons(Train train, string station, DateTime timeOper, string messageCode);
    }
}
