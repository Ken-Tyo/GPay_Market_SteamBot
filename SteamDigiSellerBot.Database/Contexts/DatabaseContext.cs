using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Models;
using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SteamDigiSellerBot.Database.Entities.Templates;
using SteamDigiSellerBot.Database.Entities.TagReplacements;

namespace SteamDigiSellerBot.Database.Contexts
{
    public class DatabaseContext : IdentityDbContext<User>
    {
        public DbSet<Bot> Bots { get; set; }
        
        public DbSet<BotSteamLicenses> BotSteamLicenses { get; set; }

        public DbSet<Game> Games { get; set; }

        public DbSet<GameSession> GameSessions { get; set; }

        public DbSet<Item> Items { get; set; }

        public DbSet<SteamProxy> SteamProxies { get; set; }

        internal DbSet<Currency> Currencies { get; set; }

        internal DbSet<CurrencyData> CurrencyData { get; set; }

        public DbSet<VacGame> VacGames { get; set; }

        public DbSet<GamePrice> GamePrices { get; set; }

        public DbSet<GameSessionStatus> GameSessionStatuses { get; set; }
        public DbSet<UserDB> DbUsers { get; set; }
        public DbSet<Seller> Sellers { get; set; }
        public DbSet<SteamCountryCode> SteamCountryCodes { get; set; }
        //public DbSet<BotTransaction> BotTransactions { get; set; }
        public DbSet<BotRegionSetting> BotRegionSettings { get; set; }
        public DbSet<GameSessionStatusLog> GameSessionStatusLogs { get; set; }
        public DbSet<BotSendGameAttempts> BotSendGameAttempts { get; set; }
        public DbSet<GameSessionItem> GameSessionItems { get; set; }


        #region Шаблоны описания и доп. описания
        public DbSet<ItemInfoTemplate> ItemInfoTemplates { get; set; }
        public DbSet<ItemInfoTemplateValue> ItemInfoTemplateValues { get; set; }
        public DbSet<ItemAdditionalInfoTemplate> ItemAdditionalInfoTemplates { get; set; }
        public DbSet<ItemAdditionalInfoTemplateValue> ItemAdditionalInfoTemplateValues { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<UpdateItemInfoStat> UpdateItemInfoStats { get; set; }
        #endregion

        #region Тэги
        public DbSet<MarketPlace> MarketPlaces { get; set; }
        public DbSet<TagPromoReplacement> TagPromoReplacements { get; set; }
        public DbSet<TagPromoReplacementValue> TagPromoReplacementValues { get; set; }
        public DbSet<TagTypeReplacement> TagTypeReplacements { get; set; }
        public DbSet<TagTypeReplacementValue> TagTypeReplacementValues { get; set; }
        public DbSet<TagInfoAppsReplacement> TagInfoAppsReplacements { get; set; }
        public DbSet<TagInfoAppsReplacementValue> TagInfoAppsReplacementValues { get; set; }
        public DbSet<TagInfoDlcReplacement> TagInfoDlcReplacements { get; set; }
        public DbSet<TagInfoDlcReplacementValue> TagInfoDlcReplacementValues { get; set; }
        #endregion

    
        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {
            Database.EnsureCreated();
            Database.Migrate();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<GamePrice>()
                .HasIndex(new string[] { "GameId", "SteamCurrencyId" }, "gameprices_un")
                .IsUnique();

            builder.Entity<GameSession>()
                .HasIndex(new string[] { "UniqueCode" }, "gamesessions_un")
                .IsUnique();

            builder.Entity<ItemAdditionalInfoTemplateValue>()
                .HasKey(x => new { x.ItemAdditionalInfoTemplateId, x.LanguageCode });

            builder.Entity<ItemInfoTemplateValue>()
                .HasKey(x => new { x.ItemInfoTemplateId, x.LanguageCode });

            builder.Entity<ItemInfoTemplate>()
                .HasMany(x => x.ItemInfoTemplateValues)
                .WithOne(x => x.ItemInfoTemplate);

            builder.Entity<ItemInfoTemplateValue>()
                .HasOne(x => x.Language)
                .WithMany(x => x.ItemInfoTemplateValues);

            builder.Entity<Language>()
                .HasKey(x => x.Code);

            builder.Entity<TagTypeReplacementValue>()
                .HasKey(x => new { x.TagTypeReplacementId, x.LanguageCode });

            builder.Entity<TagTypeReplacement>()
                .HasKey(x => new { x.Id });

            builder.Entity<TagTypeReplacement>()
                .HasMany(x => x.ReplacementValues)
                .WithOne(x => x.TagTypeReplacement);

            builder.Entity<TagPromoReplacementValue>()
                .HasKey(x => new { x.TagPromoReplacementId, x.LanguageCode });

            builder.Entity<TagPromoReplacement>()
                .HasKey(x => new { x.Id });

            builder.Entity<TagPromoReplacement>()
                .HasOne(x => x.MarketPlace)
                .WithMany(x => x.TagPromoReplacements);

            builder.Entity<TagPromoReplacement>()
                .HasMany(x => x.ReplacementValues)
                .WithOne(x => x.TagPromoReplacement);

            builder.Entity<TagInfoAppsReplacementValue>()
                .HasKey(x => new { x.TagInfoAppsReplacementId, x.LanguageCode });

            builder.Entity<TagInfoAppsReplacement>()
                .HasKey(x => new { x.Id });

            builder.Entity<TagInfoAppsReplacement>()
                .HasMany(x => x.ReplacementValues)
                .WithOne(x => x.TagInfoAppsReplacement);

            builder.Entity<TagInfoDlcReplacementValue>()
                .HasKey(x => new { x.TagInfoDlcReplacementId, x.LanguageCode });

            builder.Entity<TagInfoDlcReplacement>()
                .HasKey(x => new { x.Id });

            builder.Entity<TagInfoDlcReplacement>()
                .HasMany(x => x.ReplacementValues)
                .WithOne(x => x.TagInfoDlcReplacement);

            builder.Entity<UpdateItemInfoStat>().HasKey(x => x.JobCode);

            base.OnModelCreating(builder);
        }
    }
}
