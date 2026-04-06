using FluentValidation;

namespace LinkPara.Identity.Application.Features.Users.Queries.GetUserRole;

public class GetUserRoleQueryValidator : AbstractValidator<GetUserRoleQuery>
{
    public GetUserRoleQueryValidator()
    {
        RuleFor(x => x.UserId).NotNull().NotEmpty();
    }
}