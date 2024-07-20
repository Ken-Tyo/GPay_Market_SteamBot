using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using SteamDigiSellerBot.Database.Extensions;
using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Hubs;
using SteamDigiSellerBot.Network;
using SteamDigiSellerBot.Network.Services;
using SteamDigiSellerBot.Services.Implementation;
using SteamDigiSellerBot.Services.Implementation.ItemBulkUpdateService;
using SteamDigiSellerBot.Services.Interfaces;
using SteamDigiSellerBot.Utilities.Services;
using SteamDigiSellerBot.Validators;

namespace SteamDigiSellerBot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDatabaseWithIdentity(Configuration);
            services.AddSingleton<GlobalVault>();

            services.AddTransient<IBotRepository, BotRepository>();
            services.AddTransient<IBotSendGameAttemptsRepository, BotSendGameAttemptsRepository>();
            services.AddTransient<IVacGameRepository, VacGameRepository>();
            services.AddTransient<IItemRepository, ItemRepository>();
            services.AddTransient<IGameRepository, GameRepository>();
            services.AddTransient<IGameSessionRepository, GameSessionRepository>();
            services.AddTransient<IGameSessionStatusRepository, GameSessionStatusRepository>();
            services.AddTransient<ISteamProxyRepository, SteamProxyRepository>();
            services.AddTransient<ICurrencyDataRepository, CurrencyDataRepository>();
            services.AddTransient<IUserDBRepository, UserDBRepository>();
            services.AddTransient<ISteamCountryCodeRepository, SteamCountryCodeRepository>();
            services.AddTransient<IGameSessionStatusLogRepository, GameSessionStatusLogRepository>();
            services.AddTransient<IGamePriceRepository, GamePriceRepository>();
            services.AddTransient<IItemBulkUpdateService, ItemBulkUpdateService>();

            services.AddSingleton<ICryptographyUtilityService, CryptographyUtilityService>();

            services.AddSingleton<ISteamNetworkService, SteamNetworkService>();
            services.AddSingleton<IDigiSellerNetworkService, DigiSellerNetworkService>();
            services.AddSingleton<IItemNetworkService, ItemNetworkService>();
            services.AddSingleton<ICurrencyDataService, CurrencyDataService>();

            services.AddSingleton<IGameSessionService, GameSessionService>();

#if !DEBUG
            services.AddHostedService<UpdateExchangeRatesService>();
            services.AddHostedService<UpdateBotsService>();
            services.AddHostedService<ItemMonitoringService>();
            services.AddHostedService<DiscountMonitoringService>();
#endif

            services.AddSingleton<ISuperBotPool, SuperBotPool>();
            services.AddSingleton<IProxyPull, ProxyPull>();
            services.AddSingleton<IWsNotificationSender, WsNotificationSender>();
            services.AddSingleton<GameSessionCommon>();
            services.AddSingleton<GameSessionManager>();
            services.AddLogging(e => {
                e.AddConsole();
                e.AddDebug();
                });
            services.AddAutoMapper(typeof(Startup));

            services.AddControllersWithViews().AddNewtonsoftJson();
            services.AddSignalR(hubOptions =>
            {
                hubOptions.EnableDetailedErrors = true;
                //hubOptions.KeepAliveInterval = System.TimeSpan.FromMinutes(1);
            });

            AddValidators(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();
            
            app.UseSerilogRequestLogging();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapHub<AdminHub>("/adminhub");
                endpoints.MapHub<HomeHub>("/homehub");
            });
        }

        private IServiceCollection AddValidators(IServiceCollection services) =>
            services
                .AddFluentValidationAutoValidation()
                .AddValidatorsFromAssemblyContaining<BulkActionRequestValidator>();
    }
}
