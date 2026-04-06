using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Provisions.Commands.ProvisionCashback;

public class ProvisionCashbackCommandValidator : AbstractValidator<ProvisionCashbackCommand>
{
    public ProvisionCashbackCommandValidator()
    {
        RuleFor(x => x.ProvisionReference)
            .NotEmpty()
            .NotNull();
        RuleFor(x => x.UserId)
            .NotEmpty()
            .NotNull();
        RuleFor(x => x.WalletNumber)
            .NotEmpty()
            .NotNull();
        RuleFor(x => x.Amount)
            .NotEmpty()
            .NotNull()
            .GreaterThan(0);
    }
}
