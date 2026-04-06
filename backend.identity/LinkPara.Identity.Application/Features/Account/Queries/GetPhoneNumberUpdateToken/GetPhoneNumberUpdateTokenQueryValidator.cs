using FluentValidation;

namespace LinkPara.Identity.Application.Features.Account.Queries.GetPhoneNumberUpdateToken;
public class GetPhoneNumberUpdateTokenQueryValidator : AbstractValidator<GetPhoneNumberUpdateTokenQuery>
{
    public GetPhoneNumberUpdateTokenQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotNull()
            .NotEmpty();
        RuleFor(x => x.NewPhoneNumber!.Trim())
            .NotNull()
            .NotEmpty()
            .MaximumLength(20)
            .WithMessage("Invalid PhoneNumber format");
    }
}
