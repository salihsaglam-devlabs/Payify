using LinkPara.ApiGateway.Services.Card.Models.Shared;

namespace LinkPara.ApiGateway.Services.Card.Models.PaycoreCard.Response;

public class GetCardSensitiveDataResponse : PaycoreResponse
{
    public string CardNumber { get; set; }
    public string Cvv2 { get; set; }
    public int ExpireDate { get; set; }
}