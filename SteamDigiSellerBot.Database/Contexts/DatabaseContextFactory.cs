using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

// dotnet ef migrations add Init -- "Host=localhost;Port=5433;Database=SteamDigiSellerBotTest2;Username=postgres;Password=yourpass"
// dotnet ef migrations remove -- "Host=localhost;Port=5433;Database=SteamDigiSellerBotTest2;Username=postgres;Password=yourpass"
// Нужен для создания миграций - не удалять
namespace SteamDigiSellerBot.Database.Contexts
{
    public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
    {
        public DatabaseContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
            
            var connectionString = args.FirstOrDefault() ?? throw new InvalidOperationException("Connection string not provided.");
            
            optionsBuilder.UseNpgsql(
                connectionString, 
                options => options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
            );

            return new DatabaseContext(optionsBuilder.Options);
        }
        
    }
}