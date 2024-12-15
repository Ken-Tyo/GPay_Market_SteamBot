using AutoMapper;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Extensions;
using SteamDigiSellerBot.Models.Items;
using SteamDigiSellerBot.ViewModels;
using System;
using System.Linq;

namespace SteamDigiSellerBot.MappingProfiles
{
    public class ItemMappingProfile : Profile
    {
        public ItemMappingProfile()
        {
            CreateMap<AddItemViewModel, Item>().AfterMap((src, dst) => dst.DigiSellerIds = src.DigiSellerIds.Split(',').ToList());
            CreateMap<Item, AddItemViewModel>().ForMember(x => x.DigiSellerIds, x => x.MapFrom(x => string.Join(",", x.DigiSellerIds)));
            CreateMap<ItemViewModel, Item >();
            CreateMap<AddItemRequest, Item>()
                .AfterMap((src, dst) => dst.DigiSellerIds = src.DigiSellerIds.Split(',').ToList());

            CreateMap<Item, ItemViewModel>()
                .ForMember(x => x.OriginalSteamPrice, x => x.MapFrom((source, target) =>
                {
                    var osp = source.GetPrice()?.OriginalSteamPrice ?? 9999;
                    return osp;
                }))
                .ForMember(x => x.CurrentSteamPrice, x => x.MapFrom((source, target) =>
                {
                    var csp = source.GetPrice()?.CurrentSteamPrice ?? 9999;
                    return csp;
                }))
                .ForMember(x => x.IsProcessing, x => x.MapFrom((source, target) =>
                {
                    return source.IsProcessing != null && DateTime.UtcNow < source.IsProcessing;
                }))
                .ForMember(x => x.SteamCountryCodeId, x => x.MapFrom(s => s.Region.Id))
                .ForMember(x => x.DiscountEndTime, x => x.MapFrom(s => s.DiscountEndTimeUtc))
                .ForMember(x => x.LastSendedRegionCode, x => x.MapFrom((s,t) => s.LastSendedRegion?.Code))
                .ForMember(x => x.DiscountPercent, x => x.MapFrom((i, iw) =>
                {
                    if (!i.IsDiscount
                    || iw.OriginalSteamPrice == 9999
                    || iw.CurrentDigiSellerPrice == 9999
                    || iw.OriginalSteamPrice == 0)
                        return 0;

                    return (int)(((iw.OriginalSteamPrice - iw.CurrentSteamPrice) * 100) / iw.OriginalSteamPrice);
                }));
        }
    }
}
