using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;


namespace SteamDigiSellerBot.Database.Extensions
{
    public static class RolesExtension
    {
        public static async void EnsureRolesCreated(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var manager = scope.ServiceProvider.GetService<RoleManager<IdentityRole>>();
            await EnsureRoleExist(manager, "Admin");
            await EnsureRoleExist(manager, "Seller");
            await EnsureRoleExist(manager, "Customer");
        }

        private static async Task EnsureRoleExist(RoleManager<IdentityRole> manager, string name)
        {
            var role = await manager.FindByNameAsync(name);
            if (role == null)
                await manager.CreateAsync(new IdentityRole(name));
        }
    }
}
