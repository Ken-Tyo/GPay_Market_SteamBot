using System;
using AutoMapper;
using ProtoBuf.Meta;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Extensions;
using SteamDigiSellerBot.Models.Bots;
using SteamDigiSellerBot.Utilities.Services;
using SteamDigiSellerBot.ViewModels;

namespace SteamDigiSellerBot.MappingProfiles
{
    public class SteamBotMappingProfile : Profile
    {
        public SteamBotMappingProfile()
        {
            CreateMap<AddSteamBotViewModel, Bot>()
                .AfterMap<SteamBotMappingAction>(); ;

            CreateMap<EditBotRequest, Bot>()
                .AfterMap<EditBotRequestMappingAction>();
        }
    }

    public class SteamBotMappingAction : IMappingAction<AddSteamBotViewModel, Bot>
    {
        public void Process(AddSteamBotViewModel source, Bot destination, ResolutionContext context)
        {
            destination.ProxyStr = CryptographyUtilityService.Encrypt(source.Proxy);

            var maFileStr = source.MaFile.ReadAsStringAsync().Result;
            destination.MaFileStr = CryptographyUtilityService.Encrypt(maFileStr);
        }
    }

    public class EditBotRequestMappingAction : IMappingAction<EditBotRequest, Bot>
    {
        public void Process(EditBotRequest source, Bot destination, ResolutionContext context)
        {
            destination.ProxyStr = CryptographyUtilityService.Encrypt(source.Proxy);

            Console.WriteLine($"BotEncrytpZone {nameof(EditBotRequestMappingAction)} {destination.UserName} {source.Proxy}");
            if (source.MaFile != null)
            {
                var maFileStr = source.MaFile.ReadAsStringAsync().Result;
                destination.MaFileStr = CryptographyUtilityService.Encrypt(maFileStr);
            }
        }
    }
}
