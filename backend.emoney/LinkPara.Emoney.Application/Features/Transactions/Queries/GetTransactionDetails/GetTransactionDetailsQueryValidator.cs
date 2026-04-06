using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Transactions.Queries.GetTransactionDetails;

public class GetTransactionDetailsQueryValidator : AbstractValidator<GetTransactionDetailsQuery>
{
    public GetTransactionDetailsQueryValidator()
    {
        RuleFor(x => x.TransactionId)
            .NotNull()
            .NotEmpty();
    }
}