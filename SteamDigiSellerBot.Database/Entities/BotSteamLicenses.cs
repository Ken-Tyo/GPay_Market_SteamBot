using DatabaseRepository.Entities;

namespace SteamDigiSellerBot.Database.Entities
{
    public class BotSteamLicenses : BaseEntity
    {
        public uint[] SubIdList { get; set; }
        
        public uint[] AppIdList { get; set; }
    }
}