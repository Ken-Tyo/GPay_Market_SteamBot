using AutoMapper;
using SteamDigiSellerBot.Database.Entities.TagReplacements;
using SteamDigiSellerBot.Services.Implementation.TagServices.Models;
using System.Collections.Generic;

namespace SteamDigiSellerBot.Services.Implementation.TagServices.MappingProfiles
{
    public class AddOrUpdateTagTypeReplacementsCommandMappingProfile : Profile
    {
        public AddOrUpdateTagTypeReplacementsCommandMappingProfile()
        {
            CreateMap<AddOrUpdateTagReplacementsValue, TagTypeReplacementValue>()
                .ForMember(x => x.LanguageCode, x => x.MapFrom(x => x.LanguageCode))
                .ForMember(x => x.Value, x => x.MapFrom(x => x.Value));

            CreateMap<List<AddOrUpdateTagTypeReplacementsCommand>, Dictionary<bool, List<TagTypeReplacementValue>>>()
                .ConvertUsing(new AddOrUpdateTagTypeReplacementsCommandsConverter());
        }
    }

    public class AddOrUpdateTagTypeReplacementsCommandsConverter : ITypeConverter<List<AddOrUpdateTagTypeReplacementsCommand>, Dictionary<bool, List<TagTypeReplacementValue>>>
    {
        public Dictionary<bool, List<TagTypeReplacementValue>> Convert(
            List<AddOrUpdateTagTypeReplacementsCommand> source,
            Dictionary<bool, List<TagTypeReplacementValue>> destination,
            ResolutionContext context)
        {
            var result = new Dictionary<bool, List<TagTypeReplacementValue>>();

            foreach (var item in source)
            {
                result.Add(item.IsDlc, context.Mapper.Map<List<TagTypeReplacementValue>>(item.Values));
            }

            return result;
        }
    }
}
