using FluentValidation;

namespace LinkPara.Billing.Application.Features.Billing.Commands.CancelBillPayment;

public class CancelBillPaymentCommandValidator : AbstractValidator<CancelBillPaymentCommand>
{
    public CancelBillPaymentCommandValidator()
    {
        RuleFor(s => s.TransactionId).NotEmpty().NotNull();
        RuleFor(s => s.CancellationReason).MaximumLength(500);
    }
}