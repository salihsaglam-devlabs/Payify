using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels;
using MediatR;

namespace LinkPara.Card.Application.Features.PaycoreServices.CardServices.Queries.GetCardTransactions;

public class GetCardTransactionsQuery : IRequest<GetCardTransactionsResponse>
{
    // Zorunlu alanlar
    public string CardNo { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // Opsiyonel filtreler
    public int FinancialIndicator { get; set; }
    public string FinancialType { get; set; }
    public bool IsShowInStmt { get; set; }
    public decimal MaxAmnt { get; set; }
    public decimal MinAmnt { get; set; }
    public List<int> Otcs { get; set; } = new();
    public int Ots { get; set; }
    public List<string> ProcessingTxnCodes { get; set; } = new();
    public int TopRows { get; set; }
    public int TxnConfirmStat { get; set; }
    public string TxnStt { get; set; }
}

public class GetCardTransactionsQueryHandler
    : IRequestHandler<GetCardTransactionsQuery, GetCardTransactionsResponse>
{
    private readonly IPaycoreCardService _paycoreService;

    public GetCardTransactionsQueryHandler(IPaycoreCardService paycoreService)
    {
        _paycoreService = paycoreService;
    }

    public async Task<GetCardTransactionsResponse> Handle(
        GetCardTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        return await _paycoreService.GetCardTransactionsAsync(request);
    }
}
