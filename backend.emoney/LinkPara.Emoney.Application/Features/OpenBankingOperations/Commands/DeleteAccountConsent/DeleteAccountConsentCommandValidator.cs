using FluentValidation;

namespace LinkPara.Emoney.Application.Features.OpenBankingOperations.Commands.DeleteAccountConsent;

public class DeleteAccountConsentCommandValidator : AbstractValidator<DeleteAccountConsentCommand>
{
    public DeleteAccountConsentCommandValidator()
    {
        RuleFor(x => x.ConsentId).NotEmpty().NotNull();
    }
}
