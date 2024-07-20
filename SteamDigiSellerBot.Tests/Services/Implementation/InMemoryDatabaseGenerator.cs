using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using SteamDigiSellerBot.Database.Contexts;
using System;

namespace SteamDigiSellerBot.Tests.Services.Implementation
{
    internal static class InMemoryDatabaseGenerator
    {
        internal static DatabaseContext CreateAndReturn()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();

            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
            optionsBuilder.UseSqlite(connection);
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

            return new PostgreSqlDbContextMock(optionsBuilder.Options);
        }
    }
}
