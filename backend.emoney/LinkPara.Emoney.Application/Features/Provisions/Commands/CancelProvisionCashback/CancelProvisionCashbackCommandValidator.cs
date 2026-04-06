using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Provisions.Commands.CancelProvisionCashback;

public class CancelProvisionCashbackCommandValidator : AbstractValidator<CancelProvisionCashbackCommand>
{
    public CancelProvisionCashbackCommandValidator()
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
