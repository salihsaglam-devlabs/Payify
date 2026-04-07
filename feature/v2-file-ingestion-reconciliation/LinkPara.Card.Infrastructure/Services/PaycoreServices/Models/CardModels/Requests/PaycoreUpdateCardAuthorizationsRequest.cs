using LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels;

namespace LinkPara.Card.Infrastructure.Services.PaycoreServices.Models.CardModels.Requests;

public class PaycoreUpdateCardAuthorizationsRequest : CrdCardAuth
{
    public string CardNo { get; set; }
}
