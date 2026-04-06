using FluentValidation;

namespace LinkPara.Kkb.Application.Features.Kkb.Queries
{
    public class ValidateIbanQueryValidator : AbstractValidator<ValidateIbanQuery>
    {
        public ValidateIbanQueryValidator()
        {
            RuleFor(x => x.Iban).NotNull().NotEmpty();
            RuleFor(x => x.IdentityNo).NotNull().NotEmpty()
                             .Matches(@"[0-9]+$")
                             .MinimumLength(10)
                             .MaximumLength(11);
        }
    }
}
