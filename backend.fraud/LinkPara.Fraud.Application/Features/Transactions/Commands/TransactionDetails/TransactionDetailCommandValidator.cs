using FluentValidation;

namespace LinkPara.Fraud.Application.Features.Transactions.Commands.TransactionDetails;

public class TransactionDetailCommandValidator : AbstractValidator<TransactionDetailCommand>
{
	public TransactionDetailCommandValidator()
	{
        RuleFor(x => x.TransactionId).NotNull().NotEmpty();
    }
}
