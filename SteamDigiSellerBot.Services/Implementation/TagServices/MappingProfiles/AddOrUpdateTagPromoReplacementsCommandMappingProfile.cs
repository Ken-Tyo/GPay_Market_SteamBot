using AutoMapper;
using SteamDigiSellerBot.Database.Entities.TagReplacements;
using SteamDigiSellerBot.Services.Implementation.TagServices.Models;
using System.Collections.Generic;

namespace SteamDigiSellerBot.Services.Implementation.TagServices.MappingProfiles
{
    public class AddOrUpdateTagPromoReplacementsCommandMappingProfile : Profile
    {
        public AddOrUpdateTagPromoReplacementsCommandMappingProfile()
        {
            CreateMap<AddOrUpdateTagReplacementsValue, TagPromoReplacementValue>()
                .ForMember(x => x.LanguageCode, x => x.MapFrom(x => x.LanguageCode))
                .ForMember(x => x.Value, x => x.MapFrom(x => x.Value));

            CreateMap<List<AddOrUpdateTagPromoReplacementsCommand>, Dictionary<int, List<TagPromoReplacementValue>>>()
                .ConvertUsing(new AddOrUpdateTagPromoReplacementsCommandsConverter());
        }
    }

    public class AddOrUpdateTagPromoReplacementsCommandsConverter : ITypeConverter<List<AddOrUpdateTagPromoReplacementsCommand>, Dictionary<int, List<TagPromoReplacementValue>>>
    {
        public Dictionary<int, List<TagPromoReplacementValue>> Convert(
            List<AddOrUpdateTagPromoReplacementsCommand> source,
            Dictionary<int, List<TagPromoReplacementValue>> destination,
            ResolutionContext context)
        {
            var result = new Dictionary<int, List<TagPromoReplacementValue>>();

            foreach (var item in source)
            {
                result.Add(item.MarketPlaceId, context.Mapper.Map<List<TagPromoReplacementValue>>(item.Values));
            }

            return result;
        }
    }
}
