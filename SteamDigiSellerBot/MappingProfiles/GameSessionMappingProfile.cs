using AutoMapper;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Models.GameSessions;
using SteamDigiSellerBot.Models.Home;
using System;
using System.Linq;
using System.Text;

namespace SteamDigiSellerBot.MappingProfiles
{
    public class GameSessionMappingProfile : Profile
    {
        public GameSessionMappingProfile()
        {
            CreateMap<GameSession, GameSessionItemView>()
                .ForMember(t => t.Region, x => x.MapFrom((gs, gsiw) => gs.SendRegion?.Code))
                .ForMember(t => t.Status, x => x.Ignore())
                .ForMember(t => t.SteamCurrencyId, x => x.MapFrom(s => s.Item.SteamCurrencyId))
                .ForMember(t => t.GameName, x => x.MapFrom(s => s.Item.Name))
                .ForMember(t => t.ItemPrice, x => x.MapFrom((s, t) => s.ItemData?.Price))
                .ForMember(t => t.ItemSteamPercent, x => x.MapFrom((s, t) => s.ItemData?.SteamPercent))
                .ForMember(t => t.BotName, x => x.MapFrom(s => s.Bot != null ? s.Bot.UserName : ""))
                .ForMember(t=> t.BlockOrder, x=> x.MapFrom(s=> s.BlockOrder))
                .ForMember(t => t.StatusHistory, x => x.MapFrom((gs, gsiv) =>
                {
                    return gs.GameSessionStatusLogs
                                .OrderBy(s => s.InsertDate)
#if DEBUG
                                .OrderByDescending(s => s.InsertDate)
#endif
                                .GroupBy(s => s.InsertDate.Date)
                                .ToDictionary(g => g.Key, g => g.ToList());
                }))
                //если не удалось спарсить имя пользователя - ссылка битая - обнуляем
                .ForMember(t => t.SteamProfileUrl, x => x.MapFrom(s =>
                    string.IsNullOrWhiteSpace(s.SteamProfileName)
                        ? null
                        : s.SteamProfileUrl))
                ;


            CreateMap<CreateGameSessionRequest, GameSession>()
                .ForMember(t => t.Item, x => x.Ignore())
                .ForMember(t => t.AddedDateTime, x => x.Ignore())
                .ForMember(t => t.Bot, x => x.Ignore())
                //.ForMember(t => t.Status, x => x.Ignore())
                .ForMember(t => t.StatusId, x => x.Ignore())
                .ForMember(t => t.SteamProfileUrl, x => x.Ignore())
                ;

            CreateMap<GameSession, LastOrder>()
                .ForMember(t => t.GameName, x => x.MapFrom(s => s.Item.Name))
                .ForMember(t => t.Price, x => x.MapFrom(s => s.Item.CurrentDigiSellerPrice))
                .ForMember(t => t.UserName, x => x.MapFrom((s, lo) =>
                {
                    var str = s.SteamProfileName;
                    var hidePercent = 60;
                    var sb = new StringBuilder(str.Length);
                    var lengthToHide = (int)(str.Length * ((float)hidePercent / 100));

                    for (int i = 0; i < str.Length - lengthToHide - 1; i++)
                        sb.Append(str[i]);

                    for (int i = 0; i < lengthToHide; i++)
                        sb.Append('*');

                    sb.Append(str[str.Length - 1]);

                    return sb.ToString();
                }));

            CreateMap<GameSession, GameSessionInfo>()
                .ForMember(t => t.ItemName, x => x.MapFrom(s => s.Item.Name))
                .ForMember(t => t.BotUsername, x => x.MapFrom((gs, gsi) => gs.Bot?.UserName))
                .ForMember(t => t.BotName, x => x.MapFrom((gs, gsi) => gs.Bot?.PersonName))
                .ForMember(t => t.BotProfileUrl, x => x.MapFrom((gs, gsi) => 
                        gs.Bot !=null ? $"https://steamcommunity.com/profiles/{gs.Bot.SteamId}" : null))
                .ForMember(t => t.IsDlc, x => x.MapFrom(s => s.Item.IsDlc))
                .ForMember(t => t.CanResendGame, x => x.MapFrom(s => s.GameExistsRepeatSendCount < 3))
                .ForMember(t => t.DigisellerId, x => x.MapFrom(s => s.DigiSellerDealId))
                .ForMember(t => t.SessionEndTime, x => x.MapFrom((gs, gsi) =>
                {
                    DateTimeOffset? endDate = null;
                    if (gs.Item.IsDiscount)
                    {
                        endDate = new DateTimeOffset(gs.Item.DiscountEndTimeUtc, TimeSpan.Zero);
                    }
                    //если ручная сессия 
                    else if (String.IsNullOrEmpty(gs.DigiSellerDealId))
                    {
                        endDate = gs.ActivationEndDate;
                    }

                    return endDate;
                }))
                ;

        }
    }
}
