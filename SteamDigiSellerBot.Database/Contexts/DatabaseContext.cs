﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Models;
using System;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace SteamDigiSellerBot.Database.Contexts
{
    public class DatabaseContext : IdentityDbContext<User>
    {
        public DbSet<Bot> Bots { get; set; }

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
        public DbSet<SteamCountryCode> SteamCountryCodes { get; set; }
        //public DbSet<BotTransaction> BotTransactions { get; set; }
        public DbSet<BotRegionSetting> BotRegionSettings { get; set; }
        public DbSet<GameSessionStatusLog> GameSessionStatusLogs { get; set; }
        public DbSet<BotSendGameAttempts> BotSendGameAttempts { get; set; }
        public DbSet<GameSessionItem> GameSessionItems { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.EnableSensitiveDataLogging()
                .EnableDetailedErrors();
        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<GamePrice>()
                .HasIndex(new string[] { "GameId", "SteamCurrencyId" }, "gameprices_un")
                .IsUnique();

            builder.Entity<GameSession>()
                .HasIndex(new string[] { "UniqueCode" }, "gamesessions_un")
                .IsUnique();

            base.OnModelCreating(builder);
        }
    }
}
