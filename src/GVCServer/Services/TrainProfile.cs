using AutoMapper;
using GVCServer.Data.Entities;
using System.Linq;
using ModelsLibrary;

namespace GVCServer.Data
{
    public class TrainProfile : Profile
    {
        public TrainProfile()
        {
            this.CreateMap<Train, TrainModel>()
                .ForMember(ts => ts.Id, m => m.MapFrom(t => t.Uid))
                .ForMember(ts => ts.Index, m => m.MapFrom(t => string.Format($"{t.FormStation.Substring(0,4)} {t.Ordinal.ToString().PadLeft(3,'0')} {t.DestinationStation.Substring(0,4)}")))
                .ForMember(ts => ts.DateOper, m => m.MapFrom(t => t.OpTrain.First().Datop))
                .ForMember(tm => tm.Kind, m => m.MapFrom(t => t.TrainKindId));
            this.CreateMap<OpVag, WagonModel>()
                .ForMember(vm => vm.Ksob, m => m.MapFrom(v => v.NumNavigation.Ksob))
                .ForMember(vm => vm.Tvag, m => m.MapFrom(v => v.NumNavigation.Tvag))
                .ForMember(vm => vm.Kind, m => m.MapFrom(v => v.NumNavigation.Kind))
                .ForMember(vm => vm.Num, m => m.MapFrom(v => v.Num))
                .ReverseMap();

            this.CreateMap<TrainModel, Train>()
                .ForMember(t => t.Uid, m => m.MapFrom(tm => tm.Id))
                .ForMember(t => t.FormTime, m => m.MapFrom(tm => tm.DateOper))
                .ForMember(t => t.TrainKindId, m => m.MapFrom(tm => tm.Kind));
        }
    }
}
