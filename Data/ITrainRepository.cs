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
        /// Выдача ТГНЛ на поезд
        /// </summary>
        /// <param name="index">Индекс поезда</param>
        /// <param name="detailed"></param>
        /// <returns></returns>
        Task<TrainList> GetTrainAsync(string index);

        /// <summary>
        /// Корректировка сведений о составе поезда (сообщение 09, P0071, P0072, P0073)
        /// </summary>
        /// <param name="index"></param>    
        /// <param name="cars"></param>
        /// <param name="station"></param>
        /// <returns></returns>
        Task<bool> PutTrainAsync(string index, List<Vagon> cars, string station);

        /// <summary>
        /// Запись поезда из ТНГЛ (сообщение 02, P0005)
        /// </summary>
        /// <param name="trainList">Инфо по составу поезда</param>
        /// <param name="station">Станция передачи инфо</param>
        /// <returns></returns>
        Task<bool> AddTrainAsync(TrainList trainList, string station);

        /// <summary>
        /// Получение ТГНЛ на прибывающие поезда
        /// </summary>
        /// <param name="station">Станция прибытия</param>
        /// <param name="detailed">Полнота сведений о поезде</param>
        /// <returns>Информация по поезду в объеме ТГНЛ либо общие сведения</returns>
        Task<TrainSummary[]> GetComingTrainsAsync(string station);

        /// <summary>
        /// Отмена ТГНЛ (сообщение 333)
        /// </summary>
        /// <param name="index">Индекс отменяемого поезда</param>
        /// <returns></returns>
        Task<bool> DeleteTrain(string index);

        /// <summary>
        /// Оформление операции отправления поезда (200, P0002)
        /// </summary>
        /// <param name="index">Индекс поезда</param>
        /// <param name="station">Станция совершения операции</param>
        /// <returns></returns>
        Task<bool> Departure(string index, string station);

        /// <summary>
        /// Оформление операции прибытия поезда (201, P0001)
        /// </summary>
        /// <param name="index">Индекс поезда</param>
        /// <param name="station">Станция совершения операции</param>
        /// <returns></returns>
        Task<bool> Arrival(string index, string station);

        /// <summary>
        /// Оформление операции проследования поезда (202, P0003)
        /// </summary>
        /// <param name="index">Индекс поезда</param>
        /// <param name="station">Станция совершения операции</param>
        /// <returns></returns>
        Task<bool> Proceed(string index, string station);

        /// <summary>
        /// Оформление операции расформирования поезда (203, P0004)
        /// </summary>
        /// <param name="index">Индекс поезда</param>
        /// <param name="station">Станция совершения операции</param>
        /// <returns></returns>
        Task<bool> Disbanding(string index, string station);

        /// <summary>
        /// Оформление операции расформирования поезда (203, P0004)
        /// </summary>
        /// <param name="index">Индекс поезда</param>
        /// <param name="station">Станция совершения операции</param>
        /// <returns></returns>
        Task<bool> Appendix(string index, string station);
    }
}
