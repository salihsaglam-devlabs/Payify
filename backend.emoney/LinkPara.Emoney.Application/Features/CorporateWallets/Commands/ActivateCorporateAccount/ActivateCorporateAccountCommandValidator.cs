using FluentValidation;

namespace LinkPara.Emoney.Application.Features.CorporateWallets.Commands.ActivateCorporateAccount;

public class ActivateCorporateAccountCommandValidator : AbstractValidator<ActivateCorporateAccountCommand>
{
    public ActivateCorporateAccountCommandValidator()
    {
        RuleFor(x => x.Id).NotNull().NotEmpty();
    }
}
