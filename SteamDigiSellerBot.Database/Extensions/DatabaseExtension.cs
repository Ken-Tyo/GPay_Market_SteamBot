using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Models;

namespace SteamDigiSellerBot.Database.Extensions
{
    public static class DatabaseExtension
    {
        public static void AddDatabaseWithIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            string connection = "DefaultDebugConnection";

#if !DEBUG
            //connection = "DefaultConnection";
#endif

            services.AddPooledDbContextFactory<DatabaseContext>(options =>
            {
                options.EnableSensitiveDataLogging();
                //options.LogTo(Console.WriteLine, new[] { RelationalEventId.CommandExecuted });
                options.UseLazyLoadingProxies()
                    .UseNpgsql(configuration.GetConnectionString(connection), options2=> options2
                        .EnableRetryOnFailure(3, TimeSpan.FromSeconds(15), null)
                        .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
            }, poolSize: 256);

            services.AddScoped<DatabaseContext>(p => p
                .GetRequiredService<IDbContextFactory<DatabaseContext>>()
                .CreateDbContext());

            services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 4;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
                options.User.AllowedUserNameCharacters = null;
            }).AddEntityFrameworkStores<DatabaseContext>();

            services.PostConfigure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme,
   opt =>
   {
       opt.LoginPath = "/Home/Login";
   });
        }
    }
}
