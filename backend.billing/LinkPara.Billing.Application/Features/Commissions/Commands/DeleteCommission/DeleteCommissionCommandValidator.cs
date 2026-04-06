using FluentValidation;

namespace LinkPara.Billing.Application.Features.Commissions.Commands.DeleteCommission;

public class DeleteCommissionCommandValidator : AbstractValidator<DeleteCommissionCommand>
{
    public DeleteCommissionCommandValidator()
    {
        RuleFor(c => c.CommissionId).NotEmpty().NotNull();
    }
}