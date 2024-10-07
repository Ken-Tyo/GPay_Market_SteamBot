using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace SteamDigiSellerBot.Filters
{
    public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private const string AdminRoleCode = "Admin";

        public bool Authorize([NotNull] DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            return httpContext.User.Identity?.IsAuthenticated == true && httpContext.User.IsInRole(AdminRoleCode);
        }
    }
}
