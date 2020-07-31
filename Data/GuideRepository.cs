using GVCServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GVCServer.Data
{
    public class GuideRepository : IGuideRepository
    {
        private readonly IVCStorageContext _context;

        public GuideRepository(IVCStorageContext context)
        {
            _context = context;
        }

        public async Task<List<string[]>> GetPlanFormStations(string sourceStation, string[] destinationStations)
        {
            List<string[]> groupPFStations = new List<string[]>();
            foreach(string destinationStation in destinationStations)
            {
                int destinationInt;
                int.TryParse(destinationStation, out destinationInt);
                string[] target = await _context.PlanForm.Where(pf => sourceStation.Equals(pf.FormStation) && destinationInt >= pf.LowRange && destinationInt <= pf.HighRange)
                                                        .Select(pf => new string[] { destinationStation, pf.TargetStation, pf.TrainKind.ToString() })
                                                        .FirstOrDefaultAsync();
                if (target == null)
                    throw new KeyNotFoundException($"Не найдено станции плана формирования для назначения {destinationStation}");
                groupPFStations.Add(target);
            }
            return groupPFStations;
        }

        public async Task<List<Schedule>> GetSchedule(string station)
        {
            var stationSchedule = await _context.Schedule
                                          .Where(s => station.Equals(s.Station))
                                          .ToListAsync();
            if (stationSchedule.Count == 0)
                throw new KeyNotFoundException($"Не найден ГДП для станции {station}");
            return stationSchedule;
        }

        public async Task<List<Operation>> GetOperations()
        {
            var operations = await _context.Operation
                                          .ToListAsync();
            if (operations.Count == 0)
                throw new KeyNotFoundException($"Справочник не может быть получен");
            return operations;
        }

        public async Task<List<Pfclaim>> GetPlanFormClaims(string sourceStation)
        {
            var claims = _context.Pfclaim
                                .Where(s => sourceStation.Equals(s.StaForm))
                                .ToListAsync();
            if (claims.Result.Count == 0)
                throw new KeyNotFoundException($"Не найдены нормативы ПФ для станции {sourceStation}");
            return await claims;
        }

        public async Task<List<VagonKind>> GetVagonKinds()
        {
            var vagonKinds = _context.VagonKind
                                     .ToListAsync();
            if (vagonKinds.Result.Count == 0)
                throw new KeyNotFoundException("Справочник не может быть получен");
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
                throw new KeyNotFoundException("Справочник не может быть получен");
            return await trainKinds;
        }
    }
}
