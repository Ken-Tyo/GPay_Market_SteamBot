using Microsoft.Extensions.Logging;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Services.Implementation
{
    public class GiftBanService : IGiftBanService
    {
        private readonly IBotRepository _botRepository;
        private readonly IGameSessionRepository _gameSessionRepository;
        private readonly ICurrencyDataRepository _currencyDataRepository;
        private readonly ILogger<GiftBanService> _logger;

        private const decimal deltaUsd = 0.04m;
        public GiftBanService(
            IBotRepository botRepository,
            IGameSessionRepository gameSessionRepository,
            ICurrencyDataRepository currencyDataRepository,
            ILogger<GiftBanService> logger)
        {
            _botRepository = botRepository;
            _gameSessionRepository = gameSessionRepository;
            _currencyDataRepository = currencyDataRepository;
            _logger = logger;
        }

        public async Task<bool> SetRemainingGiftSum(int botId, int gsId)
        {
            await using var db = _gameSessionRepository.GetContext() as DatabaseContext;

            var bot = await _botRepository.GetByIdAsync(db, botId);
            var gs = await _gameSessionRepository.GetByIdAsync(db, gsId);
            var usdPrice = await PriceToUsd(gs);

            try
            {
                if (usdPrice >= 1.0m)
                {
                    bot.RemainingSumToGift = usdPrice - deltaUsd;
  
                    _logger.LogInformation($"{nameof(this.SetRemainingGiftSum)}: " +
                        $"Bot {bot.UserName} remaining sum was updated to {bot.RemainingSumToGift} " +
                        $"USD with purchasing GS Id = {gs.Id}");
                }
                else
                {
                    bot.State = Database.Enums.BotState.limit;

                    _logger.LogInformation($"{nameof(this.SetRemainingGiftSum)}: " +
                        $"Bot {bot.UserName} state was updated to {bot.State} " +
                        $"with purchasing GS Id = {gs.Id}");
                }

                await _botRepository.EditAsync(bot);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(this.SetRemainingGiftSum)}: Error while updating bot #{bot.Id}" +
                    $" remaining sum with GS #{gs.Id}");

                return false;
            }
        }

        public async Task<bool> DecreaseRemainingGiftSum(int botId, int gsId)
        {
            await using var db = _gameSessionRepository.GetContext() as DatabaseContext;

            var bot = await _botRepository.GetByIdAsync(db, botId);
            var gs = await _gameSessionRepository.GetByIdAsync(db, gsId);

            try
            {
                var usdPrice = await PriceToUsd(gs);
                bot.RemainingSumToGift -= usdPrice;

                _logger.LogInformation($"{nameof(this.DecreaseRemainingGiftSum)}: " +
                    $"Bot {bot.UserName} remaining sum was decreased to {bot.RemainingSumToGift} " +
                    $"USD with purchasing GS Id = {gs.Id}");

                await _botRepository.EditAsync(bot);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(this.DecreaseRemainingGiftSum)}: Error while updating bot #{bot.Id}" +
                    $" remaining sum with GS #{gs.Id}");

                return false;
            }
        }

        private async Task<decimal> PriceToUsd(GameSession gs)
        {
            var localPrice = gs.Item.DigiSellerPriceWithAllSales;
            var currencyId = gs.Item.SteamCurrencyId;

            var currencyDict = await _currencyDataRepository.GetCurrencyDictionary();
            var currencyValue = currencyDict[currencyId].Value;

            if (currencyValue != 0.0m)
            {
                return (localPrice / currencyValue);
            }
            else
            {
                _logger.LogError($"{nameof(this.PriceToUsd)}: Currency value is 0.0 for GS Id = {gs.Id}");
            }

            return 0.0m;
        }
    }
}
