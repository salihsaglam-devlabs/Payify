namespace LinkPara.ApiGateway.Services.Card.Models.PaycoreCard.Request;

public class UpdateCardStatusRequest
{
    public string CardNumber { get; set; }
    public string StatusCode { get; set; }
    public string Description { get; set; }
}