using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Limits.Commands.PatchCustomTierLevel;

public class PatchCustomTierLevelCommandValidator : AbstractValidator<PatchCustomTierLevelCommand>
{
    public PatchCustomTierLevelCommandValidator()
    {
        RuleFor(u => u.Id)
            .NotNull().NotEmpty();
    }
}