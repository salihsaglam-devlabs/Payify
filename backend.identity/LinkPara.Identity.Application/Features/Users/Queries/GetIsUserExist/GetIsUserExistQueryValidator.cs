using FluentValidation;

namespace LinkPara.Identity.Application.Features.Users.Queries.GetIsUserExist;

public class GetIsUserExistQueryValidator : AbstractValidator<GetIsUserExistQuery>
{
    public GetIsUserExistQueryValidator()
    {
        RuleFor(x => x.UserName)
            .NotNull()
            .NotEmpty();
    }
}