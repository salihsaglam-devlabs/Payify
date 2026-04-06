using FluentValidation;

namespace LinkPara.Emoney.Application.Features.CorporateWallets.Commands.ActivateUser;

public class ActivateCorporateWalletUserCommandValidator : AbstractValidator<ActivateCorporateWalletUserCommand>
{
    public ActivateCorporateWalletUserCommandValidator()
    {
        RuleFor(x => x.AccountUserId).NotNull().NotEmpty();
        RuleFor(x => x.AccountId).NotNull().NotEmpty();
    }
}
