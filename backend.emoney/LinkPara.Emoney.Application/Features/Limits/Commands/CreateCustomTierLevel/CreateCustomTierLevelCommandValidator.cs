using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Limits.Commands.CreateCustomTierLevel;

public class CreateCustomTierLevelCommandValidator : AbstractValidator<CreateCustomTierLevelCommand>
{
    public CreateCustomTierLevelCommandValidator()
    {
        RuleFor(u => u.TierLevelDto.Name)
            .MaximumLength(100)
            .NotNull().NotEmpty();

        RuleFor(u => u.TierLevelDto.CurrencyCode)
            .NotNull().NotEmpty();
    }
}