using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Limits.Commands.PutTierPermission;

public class PutTierPermissionCommandValidator : AbstractValidator<PutTierPermissionCommand>
{
    public PutTierPermissionCommandValidator()
    {
        RuleFor(u => u.Id)
            .NotNull().NotEmpty();
    }
}