using GVCServer.Data.Entities;
using GVCServer.Models;
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
        Task<TrainList> GetTrainAsync(string index);

        /// <summary>
        /// Корректировка сведений о составе поезда (сообщение 09, прицепка - P0071, корректировка - P0073)
        /// </summary>
        /// <param name="index"></param>
        /// <param name="newList"></param>
        /// <param name="station"></param>
        /// <returns></returns>
        public Task<bool> CorrectVagons(string index, List<OpVag> newList, string station);

        /// <summary>
        /// Отцепка вагонов от состава поезда (сообщение 09, P0072)
        /// </summary>
        /// <param name="index"></param>
        /// <param name="vagonNums"></param>
        /// <param name="station"></param>
        /// <returns></returns>
        public Task<bool> DetachVagons(string index, string[] vagonNums, string station);

        /// <summary>
        /// Запись поезда из ТНГЛ (сообщение 02, P0005)
        /// </summary>
        /// <param name="trainList"></param>
        /// <param name="station"></param>
        /// <returns></returns>
        public Task<bool> AddTrainAsync(TrainList trainList, string station);

        /// <summary>
        /// Получение общих сведений на прибывающие поезда
        /// </summary>
        /// <param name="station"></param>
        /// <returns></returns>
        Task<TrainSummary[]> GetComingTrainsAsync(string station);

        /// <summary>
        /// Отмена последней операции с поездом (сообщение 333)
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Task<bool> DeleteTrainOperaion(string index);

        /// <summary>
        /// Отмена последней операции с вагоном (вагонами) (сообщение 333)
        /// </summary>
        /// <param name="index"></param>
        /// <param name="vagonNum"></param>
        /// <returns></returns>
        public Task<bool> DeleteVagonOperaions(string index, string vagonNum);

        /// <summary>
        /// Обработка операции следования поездом (P0001, P0002, P0003)
        /// </summary>
        /// <param name="index"></param>
        /// <param name="station"></param>
        /// <param name="timeOper"></param>
        /// <param name="codeOper"></param>
        /// <returns></returns>
        public Task<bool> ProcessTrain(string index, string station, DateTime timeOper, string codeOper);

        /// <summary>
        /// Оформление операции расформирования поезда (203, P0004)
        /// </summary>
        /// <param name="index">Индекс поезда</param>
        /// <param name="station">Станция совершения операции</param>
        /// <returns></returns>
        public Task<bool> DisbandTrain(string index, string station);
    }
}
