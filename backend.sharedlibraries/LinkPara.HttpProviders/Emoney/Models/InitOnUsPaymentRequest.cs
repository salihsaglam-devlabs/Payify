namespace LinkPara.HttpProviders.Emoney.Models;

public class InitOnUsPaymentRequest
{
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string PhoneCode { get; set; }
    public string PhoneNumber { get; set; }
    public string ConversationId { get; set; }
    public string OrderId { get; set; }
    public string MerchantName { get; set; }
    public string MerchantNumber { get; set; }
    public DateTime ExpireDate { get; set; }
    public DateTime RequestDate { get; set; }
}
