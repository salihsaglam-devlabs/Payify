using FluentValidation;
using LinkPara.Emoney.Application.Features.SavedAccounts.Commands.DeleteSavedAccount;

namespace LinkPara.Emoney.Application.Features.Limits.Commands.DeleteAccountCustomTier;
public class DeleteAccountCustomTierCommandValidator : AbstractValidator<DeleteSavedAccountCommand>
{
    public DeleteAccountCustomTierCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .NotNull();
    }
}
