using FluentValidation;

namespace LinkPara.Identity.Application.Features.Roles.Commands.DeleteRole;

public class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
{
    public DeleteRoleCommandValidator()
    {
        RuleFor(u => u.RoleId)
            .NotNull().NotEmpty();
    }
}