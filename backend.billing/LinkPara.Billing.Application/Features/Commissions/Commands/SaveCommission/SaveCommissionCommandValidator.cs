using FluentValidation;

namespace LinkPara.Billing.Application.Features.Commissions.Commands.SaveCommission;

public class SaveCommissionCommandValidator : AbstractValidator<SaveCommissionCommand>
{
    public SaveCommissionCommandValidator()
    {
        RuleFor(c => c.PaymentType).NotEmpty().NotNull();
        RuleFor(c => c.RecordStatus).NotEmpty().NotNull();
    }
}