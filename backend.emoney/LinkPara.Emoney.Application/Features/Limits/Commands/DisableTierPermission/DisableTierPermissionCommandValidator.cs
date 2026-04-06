using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Limits.Commands.DisableTierPermission;

public class DisableTierPermissionCommandValidator : AbstractValidator<DisableTierPermissionCommand>
{
    public DisableTierPermissionCommandValidator()
    {
        RuleFor(u => u.Id)
            .NotNull().NotEmpty();
    }
}