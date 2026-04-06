namespace LinkPara.Card.Infrastructure.Services.PaycoreServices.Models.CardModels.Responses;

public class PaycoreGetClearCardNoResponse
{
    public string cardNo { get; set; }
    public string cardToken { get; set; }
    public int cardUniqId { get; set; }
}