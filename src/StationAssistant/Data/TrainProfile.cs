using AutoMapper;
using StationAssistant.Data.Entities;
using ModelsLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StationAssistant.Data
{
    public class TrainProfile : Profile
    {
        public TrainProfile()
        {
            this.CreateMap<WagonModel, Vagon>()
                .ReverseMap();

            this.CreateMap<TrainModel, Train>()
                .ForMember(t => t.Uid, m => m.MapFrom(tm => tm.Id))
                .ForMember(t => t.TrainKindId, m => m.MapFrom(tl =>tl.Kind))
                .ReverseMap();

            this.CreateMap<Path, PathModel>()
                .ForMember(pm => pm.TrainLength, m => m.MapFrom(p => p.Train.Sum(t => t.Length)))
                .ForMember(pm => pm.AnyTrain, m => m.MapFrom(p => p.Train.Any()));
        }
    }
}
