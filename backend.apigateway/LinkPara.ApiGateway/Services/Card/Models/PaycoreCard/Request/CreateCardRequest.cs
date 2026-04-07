using LinkPara.ApiGateway.Services.Card.Models.Shared;

namespace LinkPara.ApiGateway.Services.Card.Models.PaycoreCard.Request;

public class CreateCardRequest
{
    public CardAccount CardAccount { get; set; }
    public string EmbossName1 { get; set; }
    public string ProductCode { get; set; }
    public CardDelivery CardDelivery { get; set; }
    public string WalletNumber { get; set; }
}