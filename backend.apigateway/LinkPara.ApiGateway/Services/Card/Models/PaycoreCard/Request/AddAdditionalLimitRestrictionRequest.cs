using LinkPara.ApiGateway.Services.Card.Models.Shared;

namespace LinkPara.ApiGateway.Services.Card.Models.PaycoreCard.Request;

public class AddAdditionalLimitRestrictionRequest
{
    public string CardNo { get; set; }
    public AdditionalLimit[] AdditionalLimits { get; set; }
    public LimitRestriction[] LimitRestrictions { get; set; }
}