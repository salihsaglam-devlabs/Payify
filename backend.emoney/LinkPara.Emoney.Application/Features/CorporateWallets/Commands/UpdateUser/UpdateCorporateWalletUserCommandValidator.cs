using FluentValidation;

namespace LinkPara.Emoney.Application.Features.CorporateWallets.Commands.UpdateUser;

public class UpdateCorporateWalletUserCommandValidator :AbstractValidator<UpdateCorporateWalletUserCommand>
{
    public UpdateCorporateWalletUserCommandValidator()
    {
        RuleFor(x => x.AccountUserId).NotNull().NotEmpty();
        RuleFor(x => x.AccountId).NotNull().NotEmpty();
    }
}
