using FluentValidation;

namespace LinkPara.Identity.Application.Features.Users.Commands.AssignUserRole;

public class AssignUserRoleCommandValidator : AbstractValidator<AssignUserRoleCommand>
{
    public AssignUserRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.Roles)
            .NotNull()
            .NotEmpty();
    }
}