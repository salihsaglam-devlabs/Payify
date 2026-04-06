using LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels;

namespace LinkPara.Card.Infrastructure.Services.PaycoreServices.Models.CardModels.Requests;

public class PaycoreAddAdditionalLimitRestrictionRequest
{
    public string CardNo { get; set; }
    public AdditionalLimit[] AdditionalLimits { get; set; }
    public LimitRestriction[] LimitRestrictions { get; set; }
}
