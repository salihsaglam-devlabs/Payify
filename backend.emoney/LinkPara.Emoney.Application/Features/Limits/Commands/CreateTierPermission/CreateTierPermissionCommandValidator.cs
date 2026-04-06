using FluentValidation;
using LinkPara.Emoney.Domain.Entities;

namespace LinkPara.Emoney.Application.Features.Limits.Commands.CreateTierPermission;

public class CreateTierPermissionCommandValidator : AbstractValidator<TierPermission>
{
    public CreateTierPermissionCommandValidator()
    {
        RuleFor(s => s.TierLevel)
            .NotNull()
            .NotEmpty();

        RuleFor(s => s.PermissionType)
            .NotNull()
            .NotEmpty();

        RuleFor(s => s.IsEnabled)
            .NotNull()
            .NotEmpty();
    }
}