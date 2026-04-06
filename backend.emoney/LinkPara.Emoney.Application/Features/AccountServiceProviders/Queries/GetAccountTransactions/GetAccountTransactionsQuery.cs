using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.AccountServiceProviders.Queries.GetAccountTransactions;
public class GetAccountTransactionsQuery : IRequest<AccountTransactionsDto>
{
    public Guid AccountId { get; set; }
    public string AccountRef { get; set; }
    public string TransactionStartTime { get; set; }
    public string TransactionEndTime { get; set; }
    public string MinTransactionAmount { get; set; }
    public string MaxTransactionAmount { get; set; }
    public string TransactionType { get; set; } 
    public string Page { get; set; }
    public string Size { get; set; }
    public string OrderBy { get; set; }
    public string SortBy { get; set; }
}

public class GetAccountTransactionsQueryHandler : IRequestHandler<GetAccountTransactionsQuery, AccountTransactionsDto>
{
    private readonly IOpenBankingService _openBankingService;

    public GetAccountTransactionsQueryHandler(
        IOpenBankingService openBankingService)
    {
        _openBankingService = openBankingService;
    }

    public async Task<AccountTransactionsDto> Handle(GetAccountTransactionsQuery request,
        CancellationToken cancellationToken)
    {

        return await _openBankingService.GetAccountTransactionsAsync(request);
    }
}
