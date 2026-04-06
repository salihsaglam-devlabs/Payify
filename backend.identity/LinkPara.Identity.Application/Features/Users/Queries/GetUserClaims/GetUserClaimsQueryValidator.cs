using FluentValidation;

namespace LinkPara.Identity.Application.Features.Users.Queries.GetUserClaims;

public class GetUserClaimsQueryValidator : AbstractValidator<GetUserClaimsQuery>
{
    public GetUserClaimsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotNull()
            .NotEmpty();
    }
}