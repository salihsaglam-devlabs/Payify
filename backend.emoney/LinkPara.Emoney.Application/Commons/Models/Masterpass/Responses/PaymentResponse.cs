namespace LinkPara.Emoney.Application.Commons.Models.Masterpass.Responses;

public class PaymentResponse
{
    public int MerchantId { get; set; }
    public string OrderId { get; set; }
    public string MaskedNumber { get; set; }
    public string TransactionToken { get; set; }
    public string ExpirationDate { get; set; }
    public string PreauthStatus { get; set; }
}
