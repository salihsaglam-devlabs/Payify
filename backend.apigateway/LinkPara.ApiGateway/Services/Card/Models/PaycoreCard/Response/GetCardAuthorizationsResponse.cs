using LinkPara.ApiGateway.Services.Card.Models.Shared;

namespace LinkPara.ApiGateway.Services.Card.Models.PaycoreCard.Response;

public class GetCardAuthorizationsResponse : PaycoreResponse
{
    public CrdCardAuth CrdCardAuth { get; set; }
}