using FluentValidation;

namespace LinkPara.Emoney.Application.Features.ConsentOperations.Commands.CancelConsent;

public class CancelConsentCommandValidator : AbstractValidator<CancelConsentCommand>
{
    public CancelConsentCommandValidator()
    {
        RuleFor(x => x.ConsentId).NotEmpty().NotNull();
        RuleFor(x => x.Username).NotEmpty().NotNull();
        RuleFor(x => x.RevokeCode).NotEmpty().NotNull();
        RuleFor(x => x.ConsentTypeValue).NotEmpty().NotNull();
        RuleFor(x => x.IsDecoupledAuth).NotEmpty().NotNull();

    }
}
