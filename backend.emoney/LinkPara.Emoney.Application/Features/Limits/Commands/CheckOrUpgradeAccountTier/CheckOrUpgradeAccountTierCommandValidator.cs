using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Limits.Commands.CheckOrUpgradeAccountTier;

public class CheckOrUpgradeAccountTierCommandValidator : AbstractValidator<CheckOrUpgradeAccountTierCommand>
{
    public CheckOrUpgradeAccountTierCommandValidator()
    {
        RuleFor(u => u.AccountId)
            .NotNull()
            .NotEmpty();
        RuleFor(u => u.ValidationType)
            .NotNull()
            .NotEmpty();
    }
}