using DatabaseRepository.Repositories;
using Microsoft.EntityFrameworkCore;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Database.Repositories
{
    public interface IGameSessionStatusRepository : IBaseRepository<GameSessionStatus>
    {
        Task<Dictionary<int, GameSessionStatus>> GetGameSessionStatuses();
    }

    public class GameSessionStatusRepository : BaseRepository<GameSessionStatus>, IGameSessionStatusRepository
    {
        private IDbContextFactory<DatabaseContext> dbContextFactory;

        public GameSessionStatusRepository(IDbContextFactory<DatabaseContext> dbContextFactory)
            : base(dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
        }

        //#E13F29 - красный
        //#D3AE29 - оранжевый
        //#DDE11C - желтый
        //#4FBD53 - зеленый
        private List<GameSessionStatus> InitData = new List<GameSessionStatus>
        {
            new GameSessionStatus{ StatusId = 0, Name = "Неизвестный статус", Color = "#E13F29" },
            new GameSessionStatus{ StatusId = 1, Name = "Заказ выполнен", Color = "#4FBD53", Description = "Вы вручную подтвердили выполнение данного заказа" },
            new GameSessionStatus{ StatusId = 2, Name = "Игра получена", Color = "#4FBD53" },
            new GameSessionStatus{ StatusId = 3, Name = "Некорректный профиль", Color = "#D3AE29", Description = "Получателем был указан некорректный профиль" },
            new GameSessionStatus{ StatusId = 4, Name = "Заявка отклонена", Color = "#D3AE29" },
            new GameSessionStatus{ StatusId = 5, Name = "Некорректный регион", Color = "#E13F29" },
            new GameSessionStatus{ StatusId = 6, Name = "Заявка отправлена", Color = "#DDE11C" },
            new GameSessionStatus{ StatusId = 7, Name = "Неизвестная ошибка", Color = "#E13F29" },
            new GameSessionStatus{ StatusId = 8, Name = "Игра отклонена", Color = "#D3AE29", Description = "Получатель отклонил игру" },
            new GameSessionStatus{ StatusId = 9, Name = "Лимит бота (24 m.)", Color = "#D3AE29" },
            new GameSessionStatus{ StatusId = 10, Name = "Просрочено (таймер)", Color = "#E13F29", Description = "Закончился таймер на получение игры. Закрываем заказ." },
            new GameSessionStatus{ StatusId = 11, Name = "Просрочено (скидки)", Color = "#E13F29", Description = "Получатель не успел получить игру до окончания распродажи Steam. Закрываем заказ." },
            new GameSessionStatus{ StatusId = 12, Name = "Не указан профиль", Color = "#D3AE29", Description = "Получателем не был указан профиль для получения игры" },
            new GameSessionStatus{ StatusId = 13, Name = "Steam лагает x1 (12 m.)", Color = "#E13F29" },
            new GameSessionStatus{ StatusId = 14, Name = "Уже есть этот продукт", Color = "#D3AE29", Description = "У получателя уже присутствует данный продукт на аккаунте, ожидаем действий от получателя" },
            new GameSessionStatus{ StatusId = 15, Name = "Заказ закрыт", Color = "#E13F29", Description = "Вы вручную закрыли данный заказ" },
            new GameSessionStatus{ StatusId = 16, Name = "Ожидается подтверждение", Color = "#DDE11C", Description = "Ожидаем подтверждение заявки в друзья получателем" },
            new GameSessionStatus{ StatusId = 17, Name = "Бот не найден", Color = "#D3AE29" },
            new GameSessionStatus{ StatusId = 18, Name = "Отправка игры", Color = "#DDE11C" },
            new GameSessionStatus{ StatusId = 19, Name = "Очередь", Color = "#DDE11C" },
            new GameSessionStatus{ StatusId = 20, Name = "Смена бота", Color = "#DDE11C" },
            new GameSessionStatus{ StatusId = 21, Name = "Профиль подтвержден", Color = "#DDE11C", Description = "Ожидается выбора бота и отправки заявки в друзья" },
        };

        private async Task<List<GameSessionStatus>> InitGameSessionStatuses()
        {
            await using var _databaseContext = dbContextFactory.CreateDbContext();
            var statuses = await _databaseContext.GameSessionStatuses.ToListAsync();
            if (statuses.Count == InitData.Count)
                return statuses;

            var newStatuses = InitData
                .Select(s => s.StatusId)
                .Except(statuses.Select(s => s.StatusId))
                .ToList();

            foreach (var sId in newStatuses)
                _databaseContext.GameSessionStatuses.Add(InitData.First(s => s.StatusId == sId));
            await _databaseContext.SaveChangesAsync();

            return await _databaseContext.GameSessionStatuses.ToListAsync();
        }

        public async Task<Dictionary<int, GameSessionStatus>> GetGameSessionStatuses()
        {
            var statuses = await InitGameSessionStatuses();

            return statuses.ToDictionary(i => i.StatusId);
        }
    }
}
