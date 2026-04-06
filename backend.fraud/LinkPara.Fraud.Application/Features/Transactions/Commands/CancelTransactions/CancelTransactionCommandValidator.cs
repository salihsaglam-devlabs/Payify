using FluentValidation;

namespace LinkPara.Fraud.Application.Features.Transactions.Commands.CancelTransactions;

public class CancelTransactionCommandValidator : AbstractValidator<CancelTransactionCommand>
{
    public CancelTransactionCommandValidator()
    {
        RuleFor(x => x.TransactionId).NotNull().NotEmpty();
    }
}
