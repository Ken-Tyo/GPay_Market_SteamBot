using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SteamDigiSellerBot.ActionFilters;
using SteamDigiSellerBot.Database.Models;
using SteamDigiSellerBot.Database.Repositories;
using SteamDigiSellerBot.Models.Users;
using SteamDigiSellerBot.Utilities.Services;
using SteamDigiSellerBot.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Controllers
{
    public class UserController: Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserDBRepository _userDBRepository;

        public UserController(UserManager<User> userManager,
            IUserDBRepository userDBRepository)
        {
            _userManager = userManager;
            _userDBRepository = userDBRepository;
        }

        [HttpPost]
        [Route("user/edit/digiseller")]
        public async Task<IActionResult> EditDigisellerData(EditDigisellerDataRequest request)
        {
            await using var db = _userDBRepository.GetContext();
            User user = await _userManager.GetUserAsync(User);
            var u = await _userDBRepository.GetByAspNetUserId(user.Id);

            user.DigisellerID = CryptographyUtilityService.Encrypt(request.DigisellerID);
            user.DigisellerApiKey = CryptographyUtilityService.Encrypt(request.DigisellerApiKey);

            u.DigisellerID = CryptographyUtilityService.Encrypt(request.DigisellerID);
            u.DigisellerApiKey = CryptographyUtilityService.Encrypt(request.DigisellerApiKey);

            await _userManager.UpdateAsync(user);
            await _userDBRepository.EditAsync(db, u);

            return Ok();
        }

        [HttpPost]
        [Route("user/password"), ValidationActionFilter]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            User admin = await _userManager.GetUserAsync(User);

            if (admin != null)
            {
                IPasswordValidator<User> passwordValidator =
                    HttpContext.RequestServices.GetService(typeof(IPasswordValidator<User>)) as IPasswordValidator<User>;

                IPasswordHasher<User> passwordHasher =
                    HttpContext.RequestServices.GetService(typeof(IPasswordHasher<User>)) as IPasswordHasher<User>;

                IdentityResult result =
                    await passwordValidator.ValidateAsync(_userManager, admin, request.Password);

                if (result.Succeeded)
                {
                    admin.PasswordHash = passwordHasher.HashPassword(admin, request.Password);
                    await _userManager.UpdateAsync(admin);
                    return Ok();
                }
                else
                {
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Пользователь не найден");
            }

            return NotFound();
        }

        [HttpGet]
        [Route("user")]
        public async Task<IActionResult> UserInfo()
        {
            User user = await _userManager.GetUserAsync(User);

            return Ok(new {
                digisellerId = CryptographyUtilityService.Decrypt(user.DigisellerID),
                digisellerApiKey = CryptographyUtilityService.Decrypt(user.DigisellerApiKey)
            });
        }
    }
}
