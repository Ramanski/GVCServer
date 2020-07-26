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
    }
}
