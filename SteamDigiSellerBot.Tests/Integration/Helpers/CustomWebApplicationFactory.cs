using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SteamDigiSellerBot.Tests.Integration.Helpers
{
    public class DigisellerWebApplicationFactory<TProgram>
        : WebApplicationFactory<TProgram> where TProgram : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IAuthorizationHandler>();
                services.AddScoped<IAuthorizationHandler, TestAllowAnonymous>();
            });
            
            builder.UseEnvironment("Development");
        }
    }
    public class TestAllowAnonymous : IAuthorizationHandler
    {
        Task IAuthorizationHandler.HandleAsync(AuthorizationHandlerContext context)
        {
            foreach (IAuthorizationRequirement requirement in context.PendingRequirements.ToList())
                context.Succeed(requirement); 
            return Task.CompletedTask;
        }
    }
}