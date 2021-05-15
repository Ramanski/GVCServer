﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ModelsLibrary;
using StationAssistant.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StationAssistant.Services
{
    public interface IStationDataService
    {
        public Task<List<TrainModel>> GetDepartingTrains();

        public Task UpdatePathOccupation(int pathId);

        public Task UpdatePaths(string area);

        public Task AddTrainAsync(Guid trainId, DateTime timeArrived, int pathId);

        public Task DeleteTrainAsync(Guid trainId);

        public Task<List<TrainModel>> GetAllTrainsAsync();

        public Task<PathModel> GetPathAsync(int pathId);

        public Task<List<PathModel>> GetPaths();

        public Task<Direction[]> GetDirections();

        public Task<List<PathModel>> GetPathsOnAreaAsync(string area, bool sort);

        public Task<TrainModel> GetTrainOnPath(int pathId, bool includeVagons = false);

        public Task<List<Vagon>> GetVagons();

        public Task<Vagon[]> GetVagonsOnArea(string Area);

        public Task<List<Vagon>> GetVagonsOnPath(int pathId);

        public Task<List<Vagon>> GetVagonsOfTrain(string trainIndex);

        public Task<string[]> GetAreasAsync();

        public Task RelocateTrain(string trainIndex, int pathId);

        public Task TrainDeparture(Guid trainId);

        public Task<List<PathModel>> GetAvailablePaths(TrainModel train, bool arriving, bool departing);

        public Task<TrainModel> SetDepartureRoute(TrainModel trainModel);

        public Task<List<TrainModel>> GetArrivedTrainsAsync();

        public Task<List<TrainKind>> GetTrainKinds();

        public Task UpdateTrain(TrainModel trainModel);

        public Task UpdateVagon(Vagon updatedVagon);

        public Task<List<Vagon>> DisbandTrain(TrainModel train);

        public Task FormTrain(List<Vagon> vagons, byte trainKind, bool checkPFclaims);

        public void CheckPFclaimsAsync(string destination, ref List<Vagon> vagons);
    }

    public class StationDataService : IStationDataService
    {
        private readonly StationStorageContext _context;
        private readonly IMapper _imapper;
        private readonly IGvcDataService _igvcData;
        private readonly IConfiguration _configuration;

        public StationDataService(StationStorageContext context, IMapper imapper, IGvcDataService igvcData, IConfiguration configuration)
        {
            _configuration = configuration;
            _igvcData = igvcData;
            _imapper = imapper;
            _context = context;
        }

        public async Task UpdatePathOccupation(int pathId)
        {
            Path path = await _context.Path.Include(p => p.Vagon).Where(p => pathId.Equals(p.Id)).FirstAsync();
            path.Occupation = (short)path.Vagon.Count();
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePaths(string area)
        {
            var pathsQuery = _context.Path.Select(p => new Path { Id = p.Id, Occupation = (short)p.Vagon.Count() });
            if (!string.IsNullOrEmpty(area))
                pathsQuery.Where(p => area.Equals(p.Area));
            await pathsQuery.LoadAsync();
            _context.UpdateRange(pathsQuery);
            await _context.SaveChangesAsync();
        }

        public async Task AddTrainAsync(Guid trainId, DateTime timeArrived, int pathId)
        {
            TrainModel trainModel = await _igvcData.GetTrainInfo(trainId);

            Train train = _imapper.Map<Train>(trainModel);
            train.PathId = pathId;
            train.DateOper = timeArrived;

            List<Vagon> vagons = _imapper.Map<List<Vagon>>(trainModel.Wagons);
            foreach (Vagon vagon in vagons)
            {
                vagon.TrainId = train.Uid;
                vagon.PathId = pathId;
                vagon.DateOper = timeArrived;
            }

            _context.Train.Add(train);
            _context.Vagon.AddRange(vagons);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTrain(TrainModel updatedModel)
        {
            Train train = _context.Train.Find(updatedModel.Id);
            train.Num = updatedModel.Num.ToString();
            train.ScheduleTime = updatedModel.DateOper;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateVagon(Vagon updatedVagon)
        {
            Vagon vagon = _context.Vagon
                                  .Find(updatedVagon.Num);
            Path path = vagon.Path;
            vagon.WeightNetto = updatedVagon.WeightNetto;
            vagon.Mark = updatedVagon.Mark;
            vagon.Ksob = updatedVagon.Ksob;
            vagon.Path = null;
            await _context.SaveChangesAsync();
            vagon.PathId = path.Id;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTrainAsync(Guid trainId)
        {
            Train train = await _context.Train
                                      .Where(t => t.Uid.Equals(trainId))
                                      .Include(t => t.Vagon)
                                      .FirstOrDefaultAsync();
            if (train != null)
            {
                _context.RemoveRange(train.Vagon);
                _context.Remove(train);
                await _context.SaveChangesAsync();
            }
            else throw new ArgumentNullException();
        }

        public async Task<PathModel> GetPathAsync(int pathId)
        {
            var path = await _context.Path
                                    .Where(p => p.Id == pathId)
                                    .SingleOrDefaultAsync();
            if (path.Pfdirection.HasValue)
                path.Marks = _context.Direction.Where(d => d.DirectionId == path.Pfdirection).Select(d => d.Track).FirstOrDefault();
            return _imapper.Map<PathModel>(path);
        }

        public async Task<TrainModel> SetDepartureRoute(TrainModel trainModel)
        {
            Train train = await _context.FindAsync<Train>(trainModel.Index);
            Direction dir = await _context.Direction.FindAsync(train.DestinationStation);
            string[] route = await _igvcData.GetNearestScheduleRoute(dir.DirectionId, train.TrainKindId);
            train.Num = route[0];
            train.ScheduleTime = DateTime.Parse(route[1]);
            await _context.SaveChangesAsync();
            return _imapper.Map<TrainModel>(train);
        }

        public async Task<List<PathModel>> GetPathsOnAreaAsync(string area, bool sort)
        {
            Dictionary<short, string> directions = new Dictionary<short, string>();

            var pathsQuery = _context.Path
                                    .Include(p => p.Train)
                                    .Where(p => area.Equals(p.Area));

            if (sort)
            {
                pathsQuery = pathsQuery.Where(p => p.Sort);
                directions = await _context.Direction
                    .Select(d => new Direction() { DirectionId = d.DirectionId, Track = d.Track })
                    .Distinct()
                    .ToDictionaryAsync(d => d.DirectionId, d => d.Track);
            }

            List<Path> paths = await pathsQuery.ToListAsync();
            var pathModels = _imapper.Map<List<PathModel>>(paths);
            foreach (PathModel pathModel in pathModels)
            {
                if (sort)
                    pathModel.Marks = directions[(short)paths.Find(p => p.Id == pathModel.Id).Pfdirection];
            }

            return pathModels;
        }

        public async Task<TrainModel> GetTrainOnPath(int pathId, bool includeVagons)
        {
            Train train = await _context.Train
                                        .Where(t => pathId.Equals(t.PathId))
                                        .FirstOrDefaultAsync();
            if (train != null)
            {
                TrainModel trainModel = _imapper.Map<TrainModel>(train);
                if (includeVagons)
                {
                    List<Vagon> vagons = await GetVagonsOfTrain(trainModel.Index);
                    trainModel.Wagons = _imapper.Map<List<WagonModel>>(vagons);
                }
                return trainModel;
            }
            else
            {
                return null;
            }
        }

        public async Task<List<TrainModel>> GetDepartingTrains()
        {
            List<TrainModel> arrivedTrainModels = new List<TrainModel>();
            var trains = await _context.Train
                                        .Where(t => t.FormStation.Equals(_configuration["StationCode"]))
                                        .ToListAsync();
            foreach (Train train in trains)
            {
                TrainModel trainModel = _imapper.Map<TrainModel>(train);
                trainModel.DateOper = train.ScheduleTime ?? DateTime.MinValue;
                if (train.PathId != null)
                    trainModel.Path = await GetPathAsync((int)train.PathId);
                arrivedTrainModels.Add(trainModel);
            }

            return arrivedTrainModels;
        }

        public async Task<List<TrainModel>> GetArrivedTrainsAsync()
        {
            List<TrainModel> arrivedTrainModels = new List<TrainModel>();
            var trains = await _context.Train
                                        .Where(t => t.DestinationStation.Equals(_configuration["StationCode"]))
                                        .ToListAsync();
            foreach (Train train in trains)
            {
                TrainModel trainModel = _imapper.Map<TrainModel>(train);
                if (train.PathId != null)
                    trainModel.Path = await GetPathAsync((int)train.PathId);
                arrivedTrainModels.Add(trainModel);
            }

            return arrivedTrainModels;
        }

        public async Task<List<TrainModel>> GetAllTrainsAsync()
        {
            List<Train> trains = await _context.Train
                                              .ToListAsync();
            return (trains.Any() ? _imapper.Map<List<TrainModel>>(trains) : null);
        }

        public async Task<Direction[]> GetDirections()
        {
            return await _context.Direction.ToArrayAsync();
        }

        public async Task<Vagon[]> GetVagonsOnArea(string Area)
        {
            var vagons = await _context.Vagon.Where(v => Area.Equals(v.Path.Area)).ToArrayAsync();
            return vagons;
        }

        public async Task<List<TrainKind>> GetTrainKinds()
        {
            return await _context.TrainKind.ToListAsync();
        }

        public async Task<List<Vagon>> GetVagons()
        {
            return await _context.Vagon.ToListAsync();
        }

        public async Task<List<Vagon>> GetVagonsOnPath(int pathId)
        {
            List<Vagon> vagons = await _context.Vagon
                                               .Where(v => v.PathId.Equals(pathId))
                                               .ToListAsync();
            /*vagons.ForEach(v => v.Path = null);
            vagons.ForEach(v => v.TrainIndexNavigation = null);*/

            return vagons;
        }

        public async Task<List<Vagon>> GetVagonsOfTrain(string trainIndex)
        {
            List<Vagon> vagons = await _context.Vagon.Where(v => v.TrainId.Equals(trainIndex)).ToListAsync();

            return vagons;
        }

        public async Task<string[]> GetAreasAsync()
        {
            return await _context.Path.Select(p => p.Area).Distinct().ToArrayAsync();
        }

        public async Task RelocateTrain(string trainIndex, int pathId)
        {
            int newPath = pathId;
            Train train = _context.Train.Find(trainIndex);
            int oldPath = (int)train.PathId;
            Path path = _context.Path.Find(newPath);
            if (train.Length > (path.Length - path.Occupation))
                throw new Exception("Длина состава превышает длину свободной части пути");
            train.Path = path;
            await _context.Vagon
                          .Where(v => train == v.TrainIndexNavigation)
                          .ForEachAsync(v => v.Path = path);
            await _context.SaveChangesAsync();
            await UpdatePathOccupation(oldPath);
            await UpdatePathOccupation(newPath);
        }

        public async Task TrainDeparture(Guid trainId)
        {
            Train train = _context.Train.Find(trainId);
            bool pathIsDeparting = await _context.Path
                                                 .Where(p => train.PathId.Equals(p.Id))
                                                 .Select(p => p.Departure)
                                                 .SingleAsync();
            if (!pathIsDeparting)
                throw new ArgumentException("Поезд не на пути отправления");
            List<Vagon> vagons = await _context.Vagon.Where(v => v.TrainId.Equals(train.Uid)).ToListAsync();
            List<WagonModel> WagonModel = _imapper.Map<List<WagonModel>>(vagons);
            TrainModel TrainModel = _imapper.Map<TrainModel>(train);
            TrainModel.Wagons = WagonModel;
            await _igvcData.SendDeparting(train.Uid, DateTime.Now);
            _context.RemoveRange(vagons);
            _context.Remove(train);
            await _context.SaveChangesAsync();
            await UpdatePathOccupation((int)train.PathId);
        }

        public async Task<List<PathModel>> GetPaths()
        {
            List<Path> paths = await _context.Path.ToListAsync();
            return (paths.Any() ? _imapper.Map<List<PathModel>>(paths) : null);
        }

        public async Task<List<PathModel>> GetAvailablePaths(TrainModel train, bool arriving = false, bool departing = false)
        {
            var emptyPaths = _context.Path
                                     .Where(p => p.Occupation == 0 && !p.Main && p.Length >= train.Length);

            if (arriving)
            {
                emptyPaths = emptyPaths.Where(p => p.Arrival);
            }
            if (departing)
            {
                emptyPaths = emptyPaths.Where(p => p.Departure);
            }

            if ((arriving || departing) && !string.IsNullOrEmpty(train.Num.ToString()))
            {
                if (train.Num % 2 == 0)
                    emptyPaths = emptyPaths.Where(p => p.Even);
                else
                    emptyPaths = emptyPaths.Where(p => p.Odd);
            }

            var availablePaths = emptyPaths;// await emptyPaths.ToListAsync();

            if (availablePaths.Any())
            {
                return _imapper.Map<List<PathModel>>(availablePaths);
            }
            else
            {
                throw new Exception("Не найдено свободных путей!");
            }
        }

        public async Task<List<Vagon>> DisbandTrain(TrainModel train)
        {
            List<Vagon> vagons = await _context.Vagon
                                               .Where(v => v.TrainId == train.Id)
                                               .ToListAsync();
            List<Path> sortPaths = await _context.Path
                                                 .Where(p => p.Sort)
                                                 .ToListAsync();

            List<Exception> errors = new List<Exception>();

            List<string[]> nextDestinationStations = await _igvcData.GetNextDestinationStationsAsync(vagons);

            foreach (Vagon vagon in vagons)
            {
                List<Path> planPaths;
                bool freeSpotFound = false;

                string[] targetParams = nextDestinationStations.Where(n => n[0].Equals(vagon.Destination))
                                                               .FirstOrDefault();
                vagon.PlanForm = targetParams[1];

                short directionId = _context.Direction
                                            .Where(d => d.StationDestination.Equals(vagon.PlanForm))
                                            .Select(d => d.DirectionId)
                                            .FirstOrDefault();
                planPaths = sortPaths.Where(s => s.Pfdirection == directionId)
                                     .ToList();
                if (planPaths.Count == 0)
                {
                    errors.Add(new Exception($"\nНе найдены пути для назначения {vagon.PlanForm}"));
                }

                foreach (Path planPath in planPaths)
                {
                    if (planPath.Occupation < planPath.Length)
                    {
                        vagon.Path = planPath;
                        planPath.Occupation++;
                        freeSpotFound = true;
                        break;
                    }
                    else
                    {
                        sortPaths.Remove(planPath);
                    }
                }
                if (!freeSpotFound)
                    errors.Add(new Exception($"\nНе удалось найти путь для расформирования вагона {vagon.Num} назначением на {vagon.PlanForm}."));
            }
            if (errors.Any())
                throw new AggregateException(errors);
            vagons.ForEach(v => v.TrainIndexNavigation = null);
            _context.Path.Find(vagons[0].PathId).Occupation = 0;
            _context.Train.Remove(_context.Train.Find(train.Index));
            await _context.SaveChangesAsync();
            return vagons;
        }

        public async Task FormTrain(List<Vagon> vagons, byte trainKind, bool checkPFclaims = true)
        {
            string destination = vagons[0].PlanForm;
            byte sequenceNum = 1;

            // Проверка требований к составу Плана формирования
            if (checkPFclaims)
            {
                CheckPFclaimsAsync(destination, ref vagons);
            }

            Train train = new Train()
            {
                DestinationStation = destination,
                DateOper = DateTime.Now,
                FormStation = "161306",
                FormTime = DateTime.Now,
                Length = (short)vagons.Count(),
                WeightBrutto = (short)((vagons.Sum(v => v.Tvag) + vagons.Sum(v => v.WeightNetto)) / 10),
                PathId = vagons[0].PathId,
                TrainKindId = trainKind
            };
            _context.Train.Add(train);
            
            foreach (Vagon vagon in vagons)
            {
                vagon.SequenceNum = sequenceNum++;
                vagon.TrainIndexNavigation = train;
                vagon.DateOper = DateTime.Now;
            }

            List<WagonModel> WagonModels = _imapper.Map<List<WagonModel>>(vagons);
            TrainModel trainModel = _imapper.Map<TrainModel>(train);
            trainModel.Wagons = WagonModels;

            await _igvcData.SendTGNL(trainModel);
            await _context.SaveChangesAsync();
        }

        public void CheckPFclaimsAsync(string destination, ref List<Vagon> vagons)
        {
            Direction direction = _context.Direction.Find(destination);
            int length = vagons.Count();
            int weightSum = (int)(vagons.Sum(v => v.Tvag) + vagons.Sum(v => v.WeightNetto));

            if (direction == null)
                throw new ArgumentOutOfRangeException($"Не определены требования ПФ по направлению {destination}");
            if (length < direction.ReqLength)
                throw new ArgumentOutOfRangeException($"Недостаточно вагонов для формирования состава ({length}<{direction.ReqLength})");
            if (length > direction.MaxLength)
            {
                vagons = vagons.Take(direction.MaxLength).ToList();
                throw new ArgumentOutOfRangeException($"Была превышена максимально допустимая длина состава ({length}>{direction.MaxLength} вагонов)");
            }
            if (weightSum > direction.MaxWeight * 10)
            {
                throw new ArgumentOutOfRangeException($"Превышен максимально допустимый вес состава ({weightSum}>{direction.MaxWeight}).");
            }
        }

    }
}