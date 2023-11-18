using AutoMapper;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Extensions;
using SteamDigiSellerBot.Models.Bots;
using SteamDigiSellerBot.ViewModels;

namespace SteamDigiSellerBot.MappingProfiles
{
    public class SteamBotMappingProfile : Profile
    {
        public SteamBotMappingProfile()
        {
            CreateMap<AddSteamBotViewModel, Bot>()
                .ForMember(x => x.MaFileStr, x => x.MapFrom(x => x.MaFile.ReadAsStringAsync().Result))
                .ForMember(x => x.ProxyStr, x => x.MapFrom(x => x.Proxy));

            CreateMap<EditBotRequest, Bot>()
                .ForMember(x => x.MaFileStr, x => x.MapFrom(x => x.MaFile.ReadAsStringAsync().Result))
                .ForMember(x => x.ProxyStr, x => x.MapFrom(x => x.Proxy));
        }
    }
}
