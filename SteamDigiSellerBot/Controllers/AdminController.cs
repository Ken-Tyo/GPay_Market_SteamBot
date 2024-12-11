using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SteamDigiSellerBot.Database.Models;
using SteamDigiSellerBot.Extensions;
using SteamDigiSellerBot.ViewModels;
using System.Threading.Tasks;

namespace SteamDigiSellerBot.Controllers
{
    [Authorize (Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public AdminController(SignInManager<User> signInManager, UserManager<User> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [Route("admin/{**slug}")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("admin/index2")]
        public IActionResult Index2()
        {
            return View();
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                User admin = await _userManager.GetUserAsync(User);

                if (admin != null)
                {
                    IPasswordValidator<User> passwordValidator =
                        HttpContext.RequestServices.GetService(typeof(IPasswordValidator<User>)) as IPasswordValidator<User>;

                    IPasswordHasher<User> passwordHasher =
                        HttpContext.RequestServices.GetService(typeof(IPasswordHasher<User>)) as IPasswordHasher<User>;

                    IdentityResult result =
                        await passwordValidator.ValidateAsync(_userManager, admin, model.NewPassword);

                    if (result.Succeeded)
                    {
                        admin.PasswordHash = passwordHasher.HashPassword(admin, model.NewPassword);
                        await _userManager.UpdateAsync(admin);
                        return RedirectToAction(nameof(Index));
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
            }

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Redirect($"/{nameof(HomeController).GetControllerName()}");
        }
    }
}
