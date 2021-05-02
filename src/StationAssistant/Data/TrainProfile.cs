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
                .ForMember(t => t.Ordinal, m => m.MapFrom(tl => short.Parse(tl.Index.Substring(5, 3))))
                .ForMember(t => t.TrainIndex, m => m.MapFrom(tl => tl.Index))
                .ForMember(t => t.TrainKindId, m => m.MapFrom(tl =>tl.Kind));

            this.CreateMap<Train, TrainModel>()
                .ForMember(tl => tl.Index, m => m.MapFrom(t => t.TrainIndex));

            this.CreateMap<Path, PathModel>()
                .ForMember(pm => pm.TrainLength, m => m.MapFrom(p => p.Train.Sum(t => t.Length)))
                .ForMember(pm => pm.AnyTrain, m => m.MapFrom(p => p.Train.Any()));
        }
    }
}
