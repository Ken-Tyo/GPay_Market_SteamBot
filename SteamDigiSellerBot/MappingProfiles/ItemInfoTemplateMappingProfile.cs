using AutoMapper;
using SteamDigiSellerBot.Database.Entities.Templates;
using SteamDigiSellerBot.Models.ItemInfoTemplates.AddItemInfoTemplateDtos;
using SteamDigiSellerBot.Models.ItemInfoTemplates.GetItemInfoTemplateDtos;

namespace SteamDigiSellerBot.MappingProfiles
{
    public class ItemInfoTemplateMappingProfile : Profile
    {
        public ItemInfoTemplateMappingProfile()
        {
            CreateMap<AddItemInfoTemplateCommand, ItemInfoTemplate>()
                .ForMember(x => x.ItemInfoTemplateValues, opt => opt.MapFrom(s => s));

            CreateMap<ItemInfoTemplate, GetItemInfoTemplateResponse>()
                .ForMember(x => x.Id, opt => opt.MapFrom(s => s.Id));
        }
    }
}
