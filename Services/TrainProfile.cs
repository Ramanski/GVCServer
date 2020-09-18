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
                .ForMember(ts => ts.Index, m => m.MapFrom(t => string.Format($"{t.FormStation.Substring(0,4)} {t.Ordinal.ToString().PadLeft(3,'0')} {t.DestinationStation.Substring(0,4)}")))
                .ForMember(ts => ts.DateOper, m => m.MapFrom(t => t.FormTime))
                .ForMember(tm => tm.TrainKindId, m => m.MapFrom(t => t.TrainKindId))
                .ForMember(ts => ts.LastOperation, m => m.MapFrom(t => t.OpTrain.Select(o => o.KopNavigation.Mnemonic).FirstOrDefault().Trim()));

            this.CreateMap<OpVag, VagonModel>()
                .ForMember(vm => vm.Ksob, m => m.MapFrom(v => v.NumNavigation.Ksob))
                .ForMember(vm => vm.Tvag, m => m.MapFrom(v => v.NumNavigation.Tvag))
                .ForMember(vm => vm.Kind, m => m.MapFrom(v => v.NumNavigation.Kind))
                .ForMember(vm => vm.Num, m => m.MapFrom(v => v.Num))
                .ReverseMap();

            this.CreateMap<TrainModel, Train>()
                .ForMember(t => t.Ordinal, m => m.MapFrom(tl => short.Parse(tl.Index.Substring(5, 3))))
                .ForMember(t => t.FormTime, m => m.MapFrom(tm => tm.DateOper))
                .ForMember(t => t.TrainKindId, m => m.MapFrom(tm => tm.TrainKindId));
        }
    }
}
