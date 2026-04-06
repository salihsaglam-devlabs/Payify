using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Limits.Commands.PatchTierPermission;

public class PatchTierPermissionCommandValidator : AbstractValidator<PatchTierPermissionCommand>
{
    public PatchTierPermissionCommandValidator()
    {
        RuleFor(u => u.Id)
            .NotNull().NotEmpty();
    }
}