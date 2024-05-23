using DatabaseRepository.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Entities;
using SteamDigiSellerBot.Database.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Database.Repositories
{
    public interface IGameSessionRepository : IBaseRepositoryEx<GameSession>
    {
        Task<List<GameSession>> Sort(GameSessionSort gameSessionSort);
        Task<List<GameSession>> GetLastValidGameSessions(DatabaseContext context, int size = 10);
        Task<List<int>> GetGameSessionIds(DatabaseContext context, Expression<Func<GameSession, bool>> predicate);
        Task<List<int>> GetGameSessionIds(Expression<Func<GameSession, bool>> predicate);
        Task<(List<GameSession>, int)> Filter(string appId,
            string gameName,
            int? orderId,
            string profileStr,
            int? steamCurrencyId,
            string uniqueCode,
            GameSessionStatusEnum? statusId,
            int? page = 1,
            int? size = 50);

        Task<GameSession> GetForReset(DatabaseContext db, int id);
        Task UpdateQueueInfo(GameSession gs);
        Task<GameSessionStage> GetStageBy(int gsId);
        Task<List<GameSession>> GetGameSessionForPipline(Expression<Func<GameSession, bool>> predicate);
    }

    public class GameSessionRepository : BaseRepositoryEx<GameSession>, IGameSessionRepository
    {
        private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

        public GameSessionRepository(IDbContextFactory<DatabaseContext> dbContextFactory)
            : base(dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<(List<GameSession>, int)> Filter(
            string appId,
            string gameName,
            int? orderId,
            string profileStr,
            int? steamCurrencyId,
            string uniqueCode,
            GameSessionStatusEnum? statusId,
            int? page,
            int? size)
        {
            if (page == null)
                page = 1;

            if (size == null)
                size = 50;

            HashSet<string> codes = null;
            if (!string.IsNullOrWhiteSpace(uniqueCode))
            {
                codes = new HashSet<string>(uniqueCode.Replace(" ", "").Split(',', StringSplitOptions.RemoveEmptyEntries));
            }

            Expression<Func<GameSession, bool>> predicate = (gs) =>
                       (!orderId.HasValue || gs.Id == orderId.Value)
                    && (string.IsNullOrWhiteSpace(profileStr) || gs.SteamProfileUrl.Contains(profileStr))
                    && (string.IsNullOrWhiteSpace(appId) || gs.Item.AppId.Contains(appId))
                    && (string.IsNullOrWhiteSpace(gameName) || gs.Item.Name.Contains(gameName))
                    && (!steamCurrencyId.HasValue || steamCurrencyId <= 0 || steamCurrencyId == gs.Item.SteamCurrencyId)
                    && (codes == null || codes.Contains(gs.UniqueCode))
                    && (!statusId.HasValue || statusId <= 0 || statusId == gs.StatusId);

            await using var db = _dbContextFactory.CreateDbContext();
            var total = await db.GameSessions
                .CountAsync(predicate);

            var list = await db.GameSessions
                .Include(gs => gs.SendRegion)
                .Include(gs => gs.Item)
                .Include(gs => gs.ItemData)
                .Include(gs => gs.Bot)
                .Include(gs => gs.GameSessionStatusLogs)
                .Where(predicate)
                .OrderByDescending(gs => gs.AddedDateTime)
                .Skip((page.Value - 1) * size.Value)
                .Take(size.Value)
                .ToListAsync();

            return await Task.FromResult((list, total));
        }

        public async Task<List<GameSession>> Sort(GameSessionSort gameSessionSort)
        {
            switch (gameSessionSort)
            {
                default:
                case GameSessionSort.DateTime:

                    return await OrderByAsync(x => x.AddedDateTime);

                case GameSessionSort.DateTimeDesc:

                    return await OrderByDescendingAsync(x => x.AddedDateTime);

                    //case GameSessionSort.Status:

                    //    return await OrderByAsync(x => x.Status);
            }
        }

        public async Task<List<GameSession>> GetLastValidGameSessions(DatabaseContext db, int size = 10)
        {
            return await db.GameSessions
                .Where(gs => !string.IsNullOrEmpty(gs.SteamProfileName))
                .OrderByDescending(gs => gs.AddedDateTime)
                .Take(size)
                .ToListAsync();
        }

        public async Task<List<int>> GetGameSessionIds(DatabaseContext db, Expression<Func<GameSession, bool>> predicate)
        {
  
            return await db.GameSessions
                .AsNoTracking()
                .Where(predicate)
                .Select(gs => gs.Id)
                .ToListAsync();
        }

        public async Task<List<int>> GetGameSessionIds(Expression<Func<GameSession, bool>> predicate)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            return await GetGameSessionIds(db, predicate);
        }

        public async Task<List<GameSession>> GetGameSessionForPipline(Expression<Func<GameSession, bool>> predicate)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            try
            {
                return await db.GameSessions
                    .AsNoTracking()
                    .Where(predicate)
                    .Select(gs => new GameSession
                    {
                        Id = gs.Id,
                        StatusId = gs.StatusId,
                        Stage = gs.Stage,
                        AutoSendInvitationTime = gs.AutoSendInvitationTime
                    })
                    .ToListAsync();
            }
            catch
            {
                await Task.Delay(TimeSpan.FromSeconds(3));
                return await db.GameSessions
                    .AsNoTracking()
                    .Where(predicate)
                    .Select(gs => new GameSession
                    {
                        Id = gs.Id,
                        StatusId = gs.StatusId,
                        Stage = gs.Stage,
                        AutoSendInvitationTime = gs.AutoSendInvitationTime
                    })
                    .ToListAsync();
            }
        }

        public async Task<GameSession> GetForReset(DatabaseContext db, int id)
        {
            return await db.GameSessions
                .Include(gs => gs.Item)
                .ThenInclude(gs => gs.GamePrices)
                .Include(gs => gs.Bot)
                .FirstOrDefaultAsync(gs => gs.Id == id);
        }

        public async Task UpdateQueueInfo(GameSession gs)
        {
            await using var db= _dbContextFactory.CreateDbContext();
            db.Attach(gs);
            db.Entry(gs).Property(gs => gs.QueuePosition).IsModified = true;
            db.Entry(gs).Property(gs => gs.QueueWaitingMinutes).IsModified = true;
            await db.SaveChangesAsync();
        }

        public async Task<GameSessionStage> GetStageBy(int gsId)
        {
            await using var db = _dbContextFactory.CreateDbContext();
            return await db.GameSessions
                .Where(gs => gs.Id == gsId)
                .Select(gs => gs.Stage)
                .FirstAsync();
        }
    }
}
