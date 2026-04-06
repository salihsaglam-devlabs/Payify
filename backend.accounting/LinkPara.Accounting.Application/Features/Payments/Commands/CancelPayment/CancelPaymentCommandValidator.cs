using FluentValidation;

namespace LinkPara.Accounting.Application.Features.Payments.Commands.CancelPayment;

public class CancelPaymentCommandValidator : AbstractValidator<CancelPaymentCommand>
{
    public CancelPaymentCommandValidator()
    {
        RuleFor(x => x.ClientReferenceId)
           .NotEmpty()
           .NotNull();
    }
}