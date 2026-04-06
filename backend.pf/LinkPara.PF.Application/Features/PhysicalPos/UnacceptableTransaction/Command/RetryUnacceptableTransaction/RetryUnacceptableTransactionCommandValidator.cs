using FluentValidation;

namespace LinkPara.PF.Application.Features.PhysicalPos.UnacceptableTransaction.Command.RetryUnacceptableTransaction;

public class RetryUnacceptableTransactionCommandValidator : AbstractValidator<RetryUnacceptableTransactionCommand>
{
    public RetryUnacceptableTransactionCommandValidator()
    {
        RuleFor(x => x.UnacceptableTransactionId).NotNull().NotEmpty();
    }
}
