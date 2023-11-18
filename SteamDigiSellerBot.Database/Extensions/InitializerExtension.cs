using Microsoft.AspNetCore.Identity;
using SteamDigiSellerBot.Database.Helpers;
using SteamDigiSellerBot.Database.Models;
using SteamDigiSellerBot.Database.Repositories;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Database.Extensions
{
    public static class InitializerExtension
    {
        public static async Task InitializeAdmin(
            this UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IUserDBRepository userDBRepository)
        {
            var adminName = "Admin2";
            User user = await userManager.FindByNameAsync(adminName);

            if (user == null)
            {
                user = new User
                {
                    UserName = adminName,
                };

                var result = await userManager.CreateAsync(user, "1234");

                result = await userManager.AddToRoleAsync(user, RoleNamesHelper.Admin);

                await userDBRepository.AddAsync(new Entities.UserDB
                {
                    AspNetUser = user
                });
            }
        }

        public static async Task InitializeRoles(this RoleManager<IdentityRole> roleManager)
        {
            if (await roleManager.FindByNameAsync(RoleNamesHelper.Admin) == null)
            {
                await roleManager.CreateAsync(new IdentityRole(RoleNamesHelper.Admin));
            }
        }
    }
}
