using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SteamDigiSellerBot.Database.Entities
{
    /// <summary>
    /// Покупатели
    /// </summary>
    [Table("Customers")]
    public class Customer : UserDB
    {
        /// <summary>
        /// Почта
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Пароль
        /// </summary>
        public string Passsword { get; set; }

        /// <summary>
        /// Дата синхронизации со steam api
        /// </summary>
        public DateTime? SynchronizationDateSteamApi { get; set; }

        /// <summary>
        /// Баланс в рублях
        /// </summary>
        public decimal RubbleBalance { get; set; }

        /// <summary>
        /// Бонус баланс
        /// </summary>
        public decimal BonusBalance { get; set; }

        /// <summary>
        /// steam id привязанного профиля
        /// </summary>
        public string SteamId { get; set; }

        /// <summary>
        /// Последний steam регион (не путать с регионом профиля)
        /// </summary>
        public string LastSteamRegion { get; set; }

        /// <summary>
        /// URL Steam аватарки
        /// </summary>
        public string AvatarUrl { get; set; }

        /// <summary>
        /// Покупатель заблокирован
        /// </summary>
        public bool IsBlocked { get; set; }

        /// <summary>
        /// Массив игр вишлиста пользователя из steam
        /// </summary>
        public int[] SteamGamesWishList { get; set; }
    }
}
