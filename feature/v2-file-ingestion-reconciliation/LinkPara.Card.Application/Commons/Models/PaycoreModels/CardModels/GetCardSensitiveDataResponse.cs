namespace LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels;

public class GetCardSensitiveDataResponse: PaycoreResponse
{
    public string CardNo { get; set; }
    public string ExpiryDate { get; set; }
    public string Cvv2 { get; set; }
    public string CustomerNo { get; set; }
    public string BankingCustomerNo { get; set; }
}
