namespace LinkPara.ApiGateway.Services.Card.Models.PaycoreCard.Request;

public class GetCardTransactionsRequest
{
    public string CardNumber { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}