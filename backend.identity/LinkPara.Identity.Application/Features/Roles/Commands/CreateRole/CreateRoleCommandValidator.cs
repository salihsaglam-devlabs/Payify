using FluentValidation;

namespace LinkPara.Identity.Application.Features.Roles.Commands.CreateRole;

public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(u => u.Name)
            .NotNull().NotEmpty().MaximumLength(250);

        RuleFor(u => u.RoleScope)
         .NotNull().IsInEnum();
    }
}