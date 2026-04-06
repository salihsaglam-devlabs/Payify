
using FluentValidation;

namespace LinkPara.Emoney.Application.Features.CorporateWallets.Commands.DeactivateCorporateAccount;

public class DeactivateCorporateAccountCommandValidator : AbstractValidator<DeactivateCorporateAccountCommand>
{

    public DeactivateCorporateAccountCommandValidator()
    {
        RuleFor(x => x.Id).NotNull().NotEmpty();
    }
}
