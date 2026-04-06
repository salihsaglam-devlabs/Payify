using FluentValidation;


namespace LinkPara.Emoney.Application.Features.CorporateWallets.Commands.DeleteUser;

public class DeactivateCorporateWalletUserCommandValidator : AbstractValidator<DeactivateCorporateWalletUserCommand>
{
    public DeactivateCorporateWalletUserCommandValidator()
    {
        RuleFor(x => x.AccountUserId).NotNull().NotEmpty();
        RuleFor(x => x.AccountId).NotNull().NotEmpty();
    }
}
