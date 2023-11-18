using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
                options.UseLazyLoadingProxies()
                       .UseNpgsql(configuration.GetConnectionString(connection)));

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
