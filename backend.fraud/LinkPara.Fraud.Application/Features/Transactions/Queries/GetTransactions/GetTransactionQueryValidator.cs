using FluentValidation;

namespace LinkPara.Fraud.Application.Features.Transactions.Queries.GetTransactions;

public class GetTransactionQueryValidator : AbstractValidator<GetTransactionQuery>
{
    public GetTransactionQueryValidator()
    {
        RuleFor(x => x.TransactionId).NotNull().NotEmpty();
    }
}
