
using FluentValidation;

namespace LinkPara.Identity.Application.Features.Users.Queries.GetUserToken;

public class GetUserTokenQueryValidator : AbstractValidator<GetUserTokenQuery>
{
    public GetUserTokenQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotNull()
            .NotEmpty(); 
        RuleFor(x => x.SecureKey)
             .NotNull()
             .NotEmpty();
    }
}
