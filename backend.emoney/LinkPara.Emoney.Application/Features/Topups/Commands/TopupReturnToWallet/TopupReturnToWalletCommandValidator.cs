using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Topups.Commands.TopupReturnToWallet;

public class TopupReturnToWalletCommandValidator: AbstractValidator<TopupReturnToWalletCommand>
{
    public TopupReturnToWalletCommandValidator()
    {
        RuleFor(r => r.CardTopupRequestId)
        .NotNull()
        .NotEmpty();
    }
}
