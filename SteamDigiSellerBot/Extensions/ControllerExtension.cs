using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace SteamDigiSellerBot.Extensions
{
    public static class ControllerExtension
    {
        public static string GetControllerName(this string controllerFullName)
        {
            return controllerFullName.Replace(nameof(Controller), "");
        }

        public static BadRequestObjectResult CreateBadRequest(this Controller controller)
        {
            var ModelState = controller.ModelState;

            var errors = ModelState.Values.SelectMany(m => m.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList();

            return controller.BadRequest(new { errors });
        }
    }
}
