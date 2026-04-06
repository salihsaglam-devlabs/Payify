using FluentValidation;

namespace LinkPara.Identity.Application.Features.Roles.Queries.GetUsersByRoleId;

public class GetUserByRoleIdQueryValidator : AbstractValidator<GetUsersByRoleIdQuery>
{
    public GetUserByRoleIdQueryValidator()
    {
        RuleFor(x => x.RoleId).NotNull().NotEmpty();
    }
}
