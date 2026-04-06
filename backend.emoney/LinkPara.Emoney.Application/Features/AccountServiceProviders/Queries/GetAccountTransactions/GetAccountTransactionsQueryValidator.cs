using FluentValidation;

namespace LinkPara.Emoney.Application.Features.AccountServiceProviders.Queries.GetAccountTransactions;

public class GetAccountTransactionsQueryValidator : AbstractValidator<GetAccountTransactionsQuery>
{
    public GetAccountTransactionsQueryValidator()
    {
        RuleFor(x => x.AccountRef).NotEmpty().NotNull();
        RuleFor(x => x.TransactionStartTime).NotEmpty().NotNull();
        RuleFor(x => x.TransactionEndTime).NotEmpty().NotNull();
        RuleFor(x => x.MinTransactionAmount).NotEmpty().NotNull();
        RuleFor(x => x.MaxTransactionAmount).NotEmpty().NotNull();
        RuleFor(x => x.TransactionType).NotEmpty().NotNull();
        RuleFor(x => x.Size).NotEmpty().NotNull();
        RuleFor(x => x.Page).NotEmpty().NotNull();
        RuleFor(x => x.OrderBy).NotEmpty().NotNull();
        RuleFor(x => x.SortBy).NotEmpty().NotNull();
    }
}
