using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Limits.Commands.DisableCustomTierLevel;

public class DisableCustomTierLevelCommandValidator : AbstractValidator<DisableCustomTierLevelCommand>
{
    public DisableCustomTierLevelCommandValidator()
    {
        RuleFor(u => u.Id)
            .NotNull().NotEmpty();
    }
}