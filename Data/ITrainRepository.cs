using GVCServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GVCServer.Data
{
    interface ITrainRepository
    {
        Task<IEnumerable<Trains>> GetAllTrains();
        Task<Trains> GetTrainAsync(string index, bool detailed);
        Task<bool> PutTrain(string index, Trains train, string station);
        // Формирование поезда
        Task<string> AddTrain(Trains train, string station);

        // Выборка сведений по поездам для совершения операции 
        List<Trains> GetActualTrains(string station, bool detailed);
        // Удаление поезда и связанных с ним данных из БД
        Task<bool> DeleteTrain(string index);
    }
}
