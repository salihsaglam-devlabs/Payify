using FluentValidation;

namespace LinkPara.Emoney.Application.Features.OpenBankingOperations.Commands.CreateAccountConsent;

public class CreateAccountConsentCommandValidator : AbstractValidator<CreateAccountConsentCommand>
{
    public CreateAccountConsentCommandValidator()
    {
        RuleFor(x => x.HhsCode).NotEmpty().NotNull();
        RuleFor(x => x.AppUserId).NotEmpty().NotNull();
        RuleFor(x => x.ForwardType).NotEmpty().NotNull();
        RuleFor(x => x.AccessExpireDate).NotEmpty().NotNull();
        RuleFor(x => x.PermissionTypes).NotEmpty().NotNull();
        RuleFor(x => x.StatusCode).NotEmpty().NotNull();
        RuleFor(x => x.Identity).NotEmpty().NotNull();
        RuleFor(x => x.Identity.OhkTur).NotEmpty().NotNull();
        RuleFor(x => x.Identity.KmlkTur).NotEmpty().NotNull();
        RuleFor(x => x.Identity.KmlkVrs).NotEmpty().NotNull();
    }
}
