using FluentValidation;

namespace LinkPara.Identity.Application.Features.Permissions.Commands.UpdatePermission;

public class UpdatePermissionCommandValidator : AbstractValidator<UpdatePermissionCommand>
{
    public UpdatePermissionCommandValidator()
    {
        RuleFor(u => u.Id)
            .NotNull().NotEmpty();
    }
}
