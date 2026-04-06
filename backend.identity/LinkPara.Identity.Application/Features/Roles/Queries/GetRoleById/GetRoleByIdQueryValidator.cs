using FluentValidation;

namespace LinkPara.Identity.Application.Features.Roles.Queries.GetRoleById;

public class GetRoleByIdQueryValidator : AbstractValidator<GetRoleByIdQuery>
{
    public GetRoleByIdQueryValidator()
    {
        RuleFor(x => x.RoleId)
            .NotNull()
            .NotEmpty();
    }
}