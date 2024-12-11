using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SteamDigiSellerBot.Database.Repositories;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Controllers
{
    [Authorize]
    public class GameController : Controller
    {
        private readonly IGameRepository _gameRepository;
        public GameController(IGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        [HttpGet, Route("game/publishers")]
        public async Task<IActionResult> GetPublishers()
        {
            var publishers = await _gameRepository.GetPublishersAsync();
            return Ok(publishers);
        }
    }
}
