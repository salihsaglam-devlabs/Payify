using FluentValidation;

namespace LinkPara.Identity.Application.Features.Roles.Commands.UpdateRole;

public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator()
    {
        RuleFor(u => u.RoleId)
            .NotNull().NotEmpty();
        RuleFor(u => u.Name)
            .NotNull().NotEmpty();
    }
}