using AutoMapper;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.ViewModels;

namespace SteamDigiSellerBot.MappingProfiles
{
    public class GameMappingProfile : Profile
    {
        public GameMappingProfile()
        {
            CreateMap<AddGameViewModel, Game>();
            CreateMap<Game, AddGameViewModel>();
            CreateMap<Item, Game>();
        }
    }
}
