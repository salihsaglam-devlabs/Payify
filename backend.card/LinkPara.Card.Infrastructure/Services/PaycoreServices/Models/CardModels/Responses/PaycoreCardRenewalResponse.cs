using LinkPara.Card.Application.Commons.Models.PaycoreModels;

namespace LinkPara.Card.Infrastructure.Services.PaycoreServices.Models.CardModels.Responses;

public class PaycoreCardRenewalResponse : PaycoreResponse
{
    public string cardNo { get; set; }
    public int expiryDate { get; set; }
    public int panSeqNumber { get; set; }
}