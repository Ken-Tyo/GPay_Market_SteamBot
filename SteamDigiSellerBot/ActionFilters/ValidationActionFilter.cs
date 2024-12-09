using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace SteamDigiSellerBot.ActionFilters
{
    public class ValidationActionFilter: ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            var modelState = actionContext.ModelState;
            if (!modelState.IsValid)
            {
                var errorList = modelState.Values.SelectMany(m => m.Errors)
                                .Select(e => e.ErrorMessage)
                                .ToList();

                //var errorList = ModelState.ToDictionary(
                //    kvp => kvp.Key,
                //    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                //);

                actionContext.Result = new BadRequestObjectResult(new { errors = errorList });
                
               
                //actionContext.Result = new BadRequestObjectResult(modelState);
            }
                
        }
    }
}