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
        public TrainProfile()//IVCStorageContext context)
        {
            this.CreateMap<Train, TrainSummary>()
                .ForMember(ts => ts.Index, m => m.MapFrom(t => string.Format($"{t.FormStation.Substring(0,4)} {t.Ordinal.ToString().PadLeft(3,'0')} {t.DestinationStation.Substring(0,4)}")))
                .ForMember(ts => ts.LastOperation, m => m.MapFrom(t => t.OpTrain.Select(o => o.KopNavigation.Mnemonic).FirstOrDefault().Trim()))
                .ForMember(ts => ts.SourceStation, m => m.MapFrom(t => t.OpTrain.Select(o => o.SourceStation).FirstOrDefault()));

            this.CreateMap<Train, TrainList>()
                .ForMember(tl => tl.Index, m => m.MapFrom(t => string.Format($"{t.FormStation.Substring(0, 4)} {t.Ordinal.ToString().PadLeft(3, '0')} {t.DestinationStation.Substring(0, 4)}")))
                .ForMember(tl => tl.Vagons, m => m.Ignore());

            this.CreateMap<OpVag, VagonModel>()
                .ForMember(vm => vm.Ksob, m => m.MapFrom(v => v.NumNavigation.Ksob))
                .ForMember(vm => vm.Tvag, m => m.MapFrom(v => v.NumNavigation.Tvag))
                .ForMember(vm => vm.Kind, m => m.MapFrom(v => v.NumNavigation.Kind))
                .ForMember(vm => vm.Num, m => m.MapFrom(v => v.Num))
                .ReverseMap();

            this.CreateMap<TrainList, Train>()
                .ForMember(t => t.Ordinal, m => m.MapFrom(tl => short.Parse(tl.Index.Substring(5, 3))));
        }
    }
}
