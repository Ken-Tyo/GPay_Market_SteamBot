using FluentValidation;
using SteamDigiSellerBot.Models.Items;

namespace SteamDigiSellerBot.Validators
{
    public sealed class BulkActionRequestValidator : AbstractValidator<BulkActionRequest>
    {
        public BulkActionRequestValidator()
        {
            const string percentRequiredMessage = "Поле Процент от Steam или процент изменения цены является обязательным";

            RuleFor(x => x.SteamPercent)
            .NotEmpty()
            .When(x => x.IncreaseDecreasePercent is null)
            .WithMessage(percentRequiredMessage);

            RuleFor(x => x.IncreaseDecreasePercent)
            .NotEmpty()
            .When(x => x.SteamPercent is null)
            .WithMessage(percentRequiredMessage);

            RuleFor(x => x.Ids)
            .NotEmpty()
            .WithMessage("Товары должны быть выбраны");
        }
    }
}
