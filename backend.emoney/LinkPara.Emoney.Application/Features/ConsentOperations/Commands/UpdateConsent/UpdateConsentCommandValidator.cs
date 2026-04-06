using FluentValidation;
using LinkPara.Emoney.Domain.Enums;

namespace LinkPara.Emoney.Application.Features.ConsentOperations.Commands.UpdateConsent;

public class UpdateConsentCommandValidator : AbstractValidator<UpdateConsentCommand>
{
    public UpdateConsentCommandValidator()
    {
        RuleFor(x => x.ConsentId).NotEmpty().NotNull();
        RuleFor(x => x.CustomerId).NotEmpty().NotNull();
        RuleFor(x => x.ConsentTypeValue).NotEmpty().NotNull();
        RuleFor(x => x.SelectedAccountResponse).NotEmpty().NotNull();
        RuleFor(x => x.CustomerName).NotEmpty().NotNull();
        RuleFor(x => x.IdentityType).NotEmpty().NotNull();
        RuleFor(x => x.IdentityValue).NotEmpty().NotNull();
    }
}
