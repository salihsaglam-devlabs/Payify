using LinkPara.ApiGateway.Services.Card.Models.Shared;

namespace LinkPara.ApiGateway.Services.Card.Models.PaycoreCard.Request;

public class UpdateCardAuthorizationRequest
{
    public string CardNumber { get; set; }
    public CrdCardAuth CrdCardAuth { get; set; }
}