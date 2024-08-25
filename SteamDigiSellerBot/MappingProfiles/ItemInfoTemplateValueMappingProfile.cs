using AutoMapper;
using SteamDigiSellerBot.Database.Entities.Templates;
using SteamDigiSellerBot.Models.ItemInfoTemplates.AddItemInfoTemplateDtos;
using SteamDigiSellerBot.Models.ItemInfoTemplateValues.GetItemInfoTemplateValues;

namespace SteamDigiSellerBot.MappingProfiles
{
    public class ItemInfoTemplateValueMappingProfile : Profile
    {
        public ItemInfoTemplateValueMappingProfile()
        {
            CreateMap<AddItemInfoTemplateValueCommand, ItemInfoTemplateValue>()
                .ForMember(d => d.LanguageCode, opt => opt.MapFrom(s => s.LanguageCode))
                .ForMember(d => d.Value, opt => opt.MapFrom(s => s.Value));

            CreateMap<ItemInfoTemplateValue, GetItemInfoTemplateValueResponse>()
                .ForMember(d => d.LanguageCode, opt => opt.MapFrom(s => s.LanguageCode))
                .ForMember(d => d.Value, opt => opt.MapFrom(s => s.Value));
        }
    }
}
