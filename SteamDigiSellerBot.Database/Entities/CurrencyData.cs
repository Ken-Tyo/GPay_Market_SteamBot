using DatabaseRepository.Entities;
using System;
using System.Collections.Generic;

namespace SteamDigiSellerBot.Database.Entities
{
    public class CurrencyData : BaseEntity
    {
        public virtual List<Currency> Currencies { get; set; }

        public DateTime LastUpdateDateTime { get; set; }

        public CurrencyData()
        {
            Currencies = new List<Currency>();
        }
    }
}
