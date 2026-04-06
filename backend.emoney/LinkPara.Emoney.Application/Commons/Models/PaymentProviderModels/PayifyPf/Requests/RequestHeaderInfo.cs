namespace LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels.PayifyPf.Requests;

public class RequestHeaderInfo
{
    public string PublicKey { get; set; }
    public string Nonce { get; set; }
    public string Signature { get; set; }
    public string ConversationId { get; set; }
    public string MerchantNumber { get; set; }
}