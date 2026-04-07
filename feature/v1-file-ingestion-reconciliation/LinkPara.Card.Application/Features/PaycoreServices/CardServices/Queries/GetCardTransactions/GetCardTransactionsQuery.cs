using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels;
using MediatR;

namespace LinkPara.Card.Application.Features.PaycoreServices.CardServices.Queries.GetCardTransactions;

public class GetCardTransactionsQuery : IRequest<GetCardTransactionsResponse>
{
    public string CardNumber { get; set; }
}
public class GetCardTransactionQueryHandler : IRequestHandler<GetCardTransactionsQuery, GetCardTransactionsResponse>
{
    private readonly IPaycoreCardService _paycoreService;
    public GetCardTransactionQueryHandler(IPaycoreCardService paycoreService)
    {
        _paycoreService = paycoreService;
    }

    public async Task<GetCardTransactionsResponse> Handle(GetCardTransactionsQuery request, CancellationToken cancellationToken)
    {
        return await _paycoreService.GetCardTransactionsAsync(request);
    }
}
