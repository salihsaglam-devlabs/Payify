namespace LinkPara.Card.Infrastructure.Services.PaycoreServices.Models.CardModels.Responses;

public class PaycoreCardInformationResponse
{
    public PaycoreCardInformation[] CardInformation { get; set; }
}

public class PaycoreCardInformation
{
    public string bankingCustomerNo { get; set; }
    public string customerNo { get; set; }
    public string cardNo { get; set; }
    public string cvv2 { get; set; }
    public int expiryDate { get; set; }
}
