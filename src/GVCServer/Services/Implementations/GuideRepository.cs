using GVCServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using ModelsLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GVCServer.Repositories
{
    public class GuideRepository
    {
        private readonly IVCStorageContext _context;

        public GuideRepository(IVCStorageContext context)
        {
            _context = context;
        }

        public async Task<List<string[]>> GetPlanFormStations(string sourceStation, string[] destinationStations)
        {
            List<string[]> groupPFStations = new List<string[]>();
            foreach (string destinationStation in destinationStations)
            {
                if (sourceStation.Equals(destinationStation))
                {
                    groupPFStations.Add(new string[] { sourceStation, sourceStation, null });
                    continue;
                }
                int destinationInt;
                int.TryParse(destinationStation, out destinationInt);
                string[] target = await _context.PlanForm.Where(pf => sourceStation.Equals(pf.FormStation) && destinationInt >= pf.LowRange && destinationInt <= pf.HighRange)
                                                        .Select(pf => new string[] { destinationStation, pf.TargetStation, pf.TrainKind.ToString() })
                                                        .FirstOrDefaultAsync();
                if (target == null)
                    throw new RailProcessException($"Не найдено станции плана формирования для назначения {destinationStation}");
                groupPFStations.Add(target);
            }
            return groupPFStations;
        }

        public async Task<byte> GetTrainKind(string formStation, string destination)
        {
            int destinationNum = int.Parse(destination);
            byte trainKindVal = await _context.PlanForm.Where(p => p.FormStation.Equals(formStation) && destinationNum >= p.LowRange && destinationNum <= p.HighRange)
                                                 .Select(p => p.TrainKind)
                                                 .FirstOrDefaultAsync();
            if (trainKindVal == 0)
                throw new RailProcessException($"Значение рода поезда не определено для назначения {destination}");
            return trainKindVal;
        }

        public async Task<List<Schedule>> GetSchedule(string station)
        {
            var stationSchedule = await _context.Schedule
                                          .Where(s => station.Equals(s.Station))
                                          .ToListAsync();
            if (stationSchedule.Count == 0)
                throw new RailProcessException($"Не найден ГДП для станции {station}");
            return stationSchedule;
        }

        public async Task<List<Operation>> GetOperations()
        {
            var operations = await _context.Operation
                                          .ToListAsync();
            if (operations.Count == 0)
                throw new RailProcessException($"Справочник не может быть получен");
            return operations;
        }

        public async Task<TrainRoute> GetClosestDeparture(string station, int trainKind, int directionId, int minutesOffset = 30)
        {
            // TODO: merge two queries. Connect trainKind and schedule tables
            var trainKindNums = await _context.TrainKind
                                        .Where(t => t.Code == trainKind)
                                        .FirstOrDefaultAsync();

            var timeStart = (DateTime.Now.AddMinutes(minutesOffset)).TimeOfDay;
            
            var departureRoutes = await _context.Schedule
                                                          .Include(s => s.Direction)
                                                             .Where(s => s.DepartureTime != null &&
                                                                      s.DirectionId == directionId &&
                                                                      trainKindNums.TrainNumLow <= s.TrainNum &&
                                                                      trainKindNums.TrainNumHigh >= s.TrainNum &&
                                                                      s.Station.Equals(station))
                                                          .ToListAsync();
            var closestDepartureRoute = departureRoutes
                                                    .Select(s => new TrainRoute()
                                                    {
                                                        TrainNumber = s.TrainNum,
                                                        DepartureStation = s.Station,
                                                        DepartureTime = s.DepartureTime > timeStart ? DateTime.Today.Add((TimeSpan)s.DepartureTime) : DateTime.Today.AddDays(1).Add((TimeSpan)s.DepartureTime),
                                                        ArrivalStation = s.Direction.ArrivalStationId,
                                                        ArrivalTime = s.ArrivalTime > timeStart ? DateTime.Today.Add(s.ArrivalTime ?? TimeSpan.Zero) : DateTime.Today.AddDays(1).Add(s.ArrivalTime ?? TimeSpan.Zero)
                                                    })
                                                    .OrderBy(tr => tr.DepartureTime)
                                                    .FirstOrDefault();
            if (closestDepartureRoute == null)
            {
                throw new RailProcessException("Не найдено ни одной подходящей нитки графика");
            }
            return closestDepartureRoute;
        }

        public async Task<List<Pfclaim>> GetPlanFormClaims(string sourceStation)
        {
            var claims = _context.Pfclaim
                                .Where(s => sourceStation.Equals(s.StaForm))
                                .ToListAsync();
            if (claims.Result.Count == 0)
                throw new RailProcessException($"Не найдены нормативы ПФ для станции {sourceStation}");
            return await claims;
        }

        public async Task<List<VagonKind>> GetVagonKinds()
        {
            var vagonKinds = _context.VagonKind
                                     .ToListAsync();
            if (vagonKinds.Result.Count == 0)
                throw new RailProcessException("Справочник не может быть получен");
            return await vagonKinds;
        }

        public async Task<List<TrainKind>> GetTrainKinds()
        {
            var trainKinds = _context.TrainKind
                                     .Select(t => new TrainKind
                                     {
                                         Code = t.Code,
                                         Mnemocode = t.Mnemocode,
                                         Name = t.Name
                                     })
                                     .ToListAsync();
            if (trainKinds.Result.Count == 0)
                throw new RailProcessException("Справочник не может быть получен");
            return await trainKinds;
        }
    }
}
