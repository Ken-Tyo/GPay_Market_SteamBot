using DatabaseRepository.Repositories;
using SteamDigiSellerBot.Database.Contexts;
using SteamDigiSellerBot.Database.Entities;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Database.Repositories
{
    public interface IUserDBRepository : IBaseRepository<UserDB>
    {
        Task<UserDB> GetByAspNetUserId(string id);
        Task<UserDB> GetByAspNetUserName(string name);
    }

    public class UserDBRepository : BaseRepository<UserDB>, IUserDBRepository
    {
        public UserDBRepository(DatabaseContext databaseContext)
            : base(databaseContext)
        {

        }

        public async Task<UserDB> GetByAspNetUserId(string id)
        {
            return await GetByPredicateAsync(u => u.AspNetUser.Id == id);
        }

        public async Task<UserDB> GetByAspNetUserName(string name)
        {
            return await GetByPredicateAsync(u => u.AspNetUser.UserName == name);
        }
    }
}
