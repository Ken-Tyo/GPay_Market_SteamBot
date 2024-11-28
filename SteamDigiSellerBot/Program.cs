using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
using SteamDigiSellerBot.Database.Extensions;
using SteamDigiSellerBot.Database.Models;
using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Network.Services;
using SteamDigiSellerBot.Network.Services.Hangfire;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace SteamDigiSellerBot
{
    public partial class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Information)
                //.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                //.Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(
                   System.IO.Path.Combine("Logs", "info.txt"),
                   rollingInterval: RollingInterval.Day,
                   fileSizeLimitBytes: 30 * 1024 * 1024,
                   //retainedFileCountLimit: 2,
                   rollOnFileSizeLimit: true,
                   shared: true,
                   flushToDiskInterval: TimeSpan.FromSeconds(30),
                   restrictedToMinimumLevel: LogEventLevel.Information)
                .WriteTo.File(
                   System.IO.Path.Combine("Logs", "warn.txt"),
                   rollingInterval: RollingInterval.Day,
                   fileSizeLimitBytes: 30 * 1024 * 1024,
                   //retainedFileCountLimit: 2,
                   rollOnFileSizeLimit: true,
                   shared: true,
                   flushToDiskInterval: TimeSpan.FromSeconds(6),
                   restrictedToMinimumLevel: LogEventLevel.Warning)
                .WriteTo.File(
                   System.IO.Path.Combine("Logs", "error.txt"),
                   rollingInterval: RollingInterval.Day,
                   fileSizeLimitBytes: 30 * 1024 * 1024,
                   //retainedFileCountLimit: 2,
                   rollOnFileSizeLimit: true,
                   shared: true,
                   flushToDiskInterval: TimeSpan.FromSeconds(1),
                   restrictedToMinimumLevel: LogEventLevel.Error)
                .WriteTo.Logger(lc => lc
                    .Filter
                    .ByIncludingOnly(Matching.FromSource<UpdateItemsInfoService>())
                    .WriteTo
                    .File(
                       System.IO.Path.Combine("Logs", "update_item_descriptions.txt"),
                       rollingInterval: RollingInterval.Day,
                       fileSizeLimitBytes: 30 * 1024 * 1024,
                       rollOnFileSizeLimit: true,
                       shared: true,
                       flushToDiskInterval: TimeSpan.FromSeconds(1),
                       restrictedToMinimumLevel: LogEventLevel.Information))
                .WriteTo.Logger(lc => lc
                    .Filter
                    .ByIncludingOnly(Matching.FromSource<HangfireUpdateItemInfoJob>())
                    .WriteTo
                    .File(
                       System.IO.Path.Combine("Logs", "update_item_descriptions.txt"),
                       rollingInterval: RollingInterval.Day,
                       fileSizeLimitBytes: 30 * 1024 * 1024,
                       rollOnFileSizeLimit: true,
                       shared: true,
                       flushToDiskInterval: TimeSpan.FromSeconds(1),
                       restrictedToMinimumLevel: LogEventLevel.Information))
                //.WriteTo.File(
                //    System.IO.Path.Combine("Logs", "debug.txt"),
                //    rollingInterval: RollingInterval.Day,
                //    fileSizeLimitBytes: 30 * 1024 * 1024,
                //    //retainedFileCountLimit: 2,
                //    rollOnFileSizeLimit: true,
                //    shared: true,
                //    flushToDiskInterval: TimeSpan.FromSeconds(120),
                //    restrictedToMinimumLevel: LogEventLevel.Debug)
                .CreateLogger();

            try
            {
                IHost host = CreateHostBuilder(args)
                .Build();

                using (IServiceScope scope = host.Services.CreateScope())
                {
                    IServiceProvider services = scope.ServiceProvider;

                    try
                    {

                        UserManager<User> userManager = services.GetRequiredService<UserManager<User>>();
                        RoleManager<IdentityRole> roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                        var usersRepository = services.GetRequiredService<IUserDBRepository>();

                        await roleManager.InitializeRoles();
                        await userManager.InitializeAdmin(roleManager, usersRepository);

                        CultureInfo cultureInfo = new CultureInfo("ru-RU");

                        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
                        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "");
                    }
                }

                host.Run();
            }
            catch(Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .UseSerilog();
    }
}
