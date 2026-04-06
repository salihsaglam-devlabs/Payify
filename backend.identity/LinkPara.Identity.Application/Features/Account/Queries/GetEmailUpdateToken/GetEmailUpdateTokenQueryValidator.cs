using FluentValidation;

namespace LinkPara.Identity.Application.Features.Account.Queries.GetEmailUpdateToken;

public class GetEmailUpdateTokenQueryValidator : AbstractValidator<GetEmailUpdateTokenQuery>
{
    public GetEmailUpdateTokenQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.NewEmail)
            .NotNull()
            .NotEmpty()
            .MaximumLength(256)
            .EmailAddress(FluentValidation.Validators.EmailValidationMode.AspNetCoreCompatible);
    }
}
