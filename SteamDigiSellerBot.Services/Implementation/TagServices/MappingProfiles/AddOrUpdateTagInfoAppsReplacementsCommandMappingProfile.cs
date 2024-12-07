using AutoMapper;
using SteamDigiSellerBot.Database.Entities.TagReplacements;
using SteamDigiSellerBot.Services.Implementation.TagServices.Models;

namespace SteamDigiSellerBot.Services.Implementation.TagServices.MappingProfiles
{
    public class AddOrUpdateTagInfoAppsReplacementsCommandMappingProfile : Profile
    {
        public AddOrUpdateTagInfoAppsReplacementsCommandMappingProfile()
        {
            CreateMap<AddOrUpdateTagReplacementsValue, TagInfoAppsReplacementValue>()
                .ForMember(x => x.LanguageCode, x => x.MapFrom(x => x.LanguageCode))
                .ForMember(x => x.Value, x => x.MapFrom(x => x.Value));
        }
    }
}
