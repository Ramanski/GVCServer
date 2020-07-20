using AutoMapper;
using GVCServer.Data.Entities;
using GVCServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GVCServer.Data
{
    public class TrainProfile : Profile
    {
        public TrainProfile()
        {
            this.CreateMap<Train, TrainSummary>()
                .ForMember(ts => ts.Index, m => m.MapFrom(t => string.Format($"{t.FormNode} {t.Ordinal.ToString().PadLeft(3,'0')} {t.DestinationNode}")))
                .ForMember(ts => ts.LastOperation, m => m.MapFrom(t => t.OpTrain.Select(o => o.Kop).LastOrDefault()))
                .ForMember(ts => ts.SourceStation, m => m.MapFrom(t => t.OpTrain.Select(o => o.SourceStation).LastOrDefault()));

            this.CreateMap<Train, TrainList>()
              //  .ForMember(tl => tl.SourceStation, m => m.MapFrom(t => t.OpTrain.Select(o => o.SourceStation).LastOrDefault()))
                .ForMember(tl => tl.Index, m => m.MapFrom(t => string.Format($"{t.FormNode} {t.Ordinal.ToString().PadLeft(3, '0')} {t.DestinationNode}")))
                .ForMember(tl => tl.Vagons, m => m.Ignore());

            this.CreateMap<OpVag, VagonModel>()
                .ForMember(vm => vm.Ksob, m => m.MapFrom(v => v.Vagon.Ksob))
                .ForMember(vm => vm.Tvag, m => m.MapFrom(v => v.Vagon.Tvag))
                .ForMember(vm => vm.Kind, m => m.MapFrom(v => v.Vagon.Kind))
                .ReverseMap();

            this.CreateMap<TrainList, Train>()
                .ForMember(t => t.DestinationNode, m => m.MapFrom(tl => tl.Index.Substring(9, 4)))
                .ForMember(t => t.Ordinal, m => m.MapFrom(tl => Byte.Parse(tl.Index.Substring(5, 3))))
                .ForMember(t => t.FormNode, m => m.MapFrom(tl => tl.Index.Substring(0, 4)));
        }
    }
}
