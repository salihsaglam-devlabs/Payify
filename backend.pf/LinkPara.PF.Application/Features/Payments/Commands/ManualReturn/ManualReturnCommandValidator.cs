using FluentValidation;

namespace LinkPara.PF.Application.Features.Payments.Commands.ManualReturn;

public class ManualReturnCommandValidator : AbstractValidator<ManualReturnCommand>
{
    public ManualReturnCommandValidator()
    {
        RuleFor(s => s.MerchantTransactionId)
            .NotNull()
            .NotEmpty();
    }
}