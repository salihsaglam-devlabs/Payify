using FluentValidation;

namespace LinkPara.Emoney.Application.Features.OpenBankingOperations.Commands.CreateAccountConsent;

public class CreateAccountConsentCommandValidator : AbstractValidator<CreateAccountConsentCommand>
{
    public CreateAccountConsentCommandValidator()
    {        
        RuleFor(x => x.AppUserId).NotEmpty().NotNull();
    }
}
