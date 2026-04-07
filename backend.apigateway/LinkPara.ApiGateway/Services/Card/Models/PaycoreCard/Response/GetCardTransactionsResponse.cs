using LinkPara.ApiGateway.Services.Card.Models.Shared;

namespace LinkPara.ApiGateway.Services.Card.Models.PaycoreCard.Response;

public class GetCardTransactionsResponse : PaycoreResponse
{
    public List<CardTransactionResponseItem> Transactions { get; set; }
}