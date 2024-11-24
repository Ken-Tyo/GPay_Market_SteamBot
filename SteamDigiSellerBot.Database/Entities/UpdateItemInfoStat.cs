using System;

namespace SteamDigiSellerBot.Database.Entities
{
    /// <summary>
    /// Стастистика запросов при обновлении описаний.
    /// </summary>
    public class UpdateItemInfoStat
    {
        public string JobCode { get; set; }

        public DateTime UpdateDate { get; set; }

        public int RequestCount { get; set; }
    }
}
