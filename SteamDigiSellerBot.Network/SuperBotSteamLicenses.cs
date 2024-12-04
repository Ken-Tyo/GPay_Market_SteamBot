using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SteamKit2;
using Bot = SteamDigiSellerBot.Database.Entities.Bot;

namespace SteamDigiSellerBot.Network
{
    /// <summary>
    /// Синхронизирует список лицензий, которыми обладает аккаунт Steam Бота с базой данных.
    /// </summary>
    public class SuperBotSteamLicenses
    {
        private readonly Bot _bot;
        private readonly SteamApps _steamApps;
        private readonly ILogger _logger;
        
        private readonly List<uint> _subIdList = new ();
        private readonly HashSet<uint> _appIdList = new (); // Могут повторяться, поэтому HashSet
        
        /// <summary>
        /// Возвращает список идентификаторов комплектов (SubID), которыми обладает Бот.
        /// </summary>
        public uint[] SubIdList => _subIdList.ToArray();
        
        /// <summary>
        /// Возвращает список идентификаторов приложений (AppID), которыми обладает Бот.
        /// </summary>
        public uint[] AppIdList => _appIdList.ToArray();

        public SuperBotSteamLicenses(Bot bot, SteamApps steamApps, ILogger logger)
        {
            _bot = bot;
            _steamApps = steamApps;
            _logger = logger;
        }
        
        /// <summary>
        /// Парсит список лицензий, полученный от Steam и сохраняет в памяти.
        /// </summary>
        /// <param name="licenseList">Список, полученный в событии <see cref="SteamApps.LicenseListCallback"/>.</param>
        public async Task FillFromSteam(ReadOnlyCollection<SteamApps.LicenseListCallback.License> licenseList)
        {
            _logger.LogInformation($"У бота {_bot.UserName} на аккаунте зарегистрировано {licenseList.Count} лицензий.");
           
            // Получаем информацию о пакетах через PICS (Steam Product Information Control System).
            
            var requestList = licenseList.Select(
                lic => new SteamApps.PICSRequest(lic.PackageID, lic.AccessToken)
            ).ToList();
            
            var packageList = await _steamApps.PICSGetProductInfo(
                Enumerable.Empty<SteamApps.PICSRequest>(),
                requestList);

            if (packageList.Results == null)
            {
                _logger.LogError($"Не удалось получить информацию о лицензиях для бота {_bot.UserName}.");
                return;
            }
            
            // Сохраняем все Комплекты и Приложения (Игры, DLC, и т.д.) в них.

            _subIdList.Clear();
            _appIdList.Clear();

            foreach (var package in packageList.Results)
            {
                foreach (var info in package.Packages.Values)
                {
                    _subIdList.Add(info.ID);

                    foreach (var appId in info.KeyValues["appids"].Children)
                    {
                        if (appId.Value != null)
                            _appIdList.Add(uint.Parse(appId.Value));
                    }
                }
            }

            _logger.LogInformation($"У бота {_bot.UserName} на аккаунте зарегистрировано{_subIdList.Count} комплектов и {_appIdList.Count} приложений.");
        }
    }
}