using FluentValidation;
using SteamDigiSellerBot.Network.Models.UpdateItemInfoCommand;
using System.Collections.Generic;

namespace SteamDigiSellerBot.ModelValidators
{
    public sealed class UpdateItemInfoCommandValidator : AbstractValidator<List<UpdateItemInfoCommand>>
    {
        public UpdateItemInfoCommandValidator()
        {
            RuleForEach(x => x).ChildRules(t => t.RuleFor(f => f.InfoData).ChildRules(y => y.RuleForEach(z => z).ChildRules(n => n.RuleFor(g => g.Value).NotEmpty().WithMessage("Не указано описание"))));
        }
    }
}
