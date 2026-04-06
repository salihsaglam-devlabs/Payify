using FluentValidation;

namespace LinkPara.Accounting.Application.Features.Payments.Commands.DeletePayment;

public class DeletePaymentCommandValidator : AbstractValidator<DeletePaymentCommand>
{
    public DeletePaymentCommandValidator()
    {
        RuleFor(x => x.Id)
           .NotEmpty()
           .NotNull();
    }
}
