using FluentValidation;

namespace LinkPara.Identity.Application.Features.Users.Queries.GetUserById;

public class GetUserByUserNameQueryValidator : AbstractValidator<GetUserByUserNameQuery>
{
    public GetUserByUserNameQueryValidator()
    {
        RuleFor(x => x.UserName)
            .NotNull()
            .NotEmpty();
    }
}