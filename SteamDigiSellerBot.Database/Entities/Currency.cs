using DatabaseRepository.Entities;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamDigiSellerBot.Database.Entities
{
    public class Currency : BaseEntity
    {
        public string Name { get; set; }
        public int SteamId { get; set; }
        public string SteamSymbol { get; set; }

        public int Position { get; set; }

        public string Code { get; set; }
        public string CountryCode { get; set; }

        /// <summary>
        /// Value for 1 ruble
        /// </summary>
        public decimal Value { get; set; }

        ///// <summary>
        ///// Rubles for 1 value
        ///// </summary>
        //[NotMapped]
        //[JsonIgnore]
        //public decimal RublesValue
        //{
        //    get
        //    {
        //        return Math.Round(1 / Value, 2);
        //    }
        //}

        //public Currency GetDefault()
        //{
        //    Code = "RUB";
        //    SteamSymbol = "руб.";
        //    Value = 1;

        //    return this;
        //}
    }
}
