using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetCardTransactions;
public class GetCardTransactionsQuery : IRequest<CardTransactionsResultDto>
{
    public string HhsCode { get; set; }
    public string AppUserId { get; set; }
    public string ConsentId { get; set; }
    public string CardRefNo { get; set; }
    public int PeriodValue { get; set; }
    public string StatementType { get; set; }
    public int PageRecordCount { get; set; }
    public int PageNo { get; set; }
    public string DebtOrCredit { get; set; }
    public string OrderType { get; set; }
}

public class GetCardsQueryHandler : IRequestHandler<GetCardTransactionsQuery, CardTransactionsResultDto>
{
    private readonly IOpenBankingOperationsService _openBankingOperationsService;

    public GetCardsQueryHandler(
         IOpenBankingOperationsService openBankingOperationsService)
    {
        _openBankingOperationsService = openBankingOperationsService;
    }

    public async Task<CardTransactionsResultDto> Handle(GetCardTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        return await _openBankingOperationsService.GetCardTransactionsAsync(request);
    }
}
