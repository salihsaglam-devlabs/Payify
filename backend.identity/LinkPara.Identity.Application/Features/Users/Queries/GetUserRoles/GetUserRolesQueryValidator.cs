using FluentValidation;

namespace LinkPara.Identity.Application.Features.Users.Queries.GetUserRole;

public class GetUserRolesQueryValidator : AbstractValidator<GetUserRoleQuery>
{
    public GetUserRolesQueryValidator()
    {
        RuleFor(x => x.UserId).NotNull().NotEmpty();
    }
}