namespace LinkPara.HttpProviders.PF.Models.Request;

public class VerifyOnUsPaymentRequest
{
    public bool IsVerifiedByUser { get; set; }
    public string OrderId { get; set; }
    public string ConversationId { get; set; }
    public string MerchantNumber { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
    public string PhoneCode { get; set; }
    public string PhoneNumber { get; set; }
    public string WalletNumber { get; set; }
    public string LanguageCode { get; set; }
    public string ClientIpAddress { get; set; }
}