namespace LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels.PayifyPf.Requests;

public class SignatureDataRequest
{
    public string PublicKey { get; set; }
    public string MerchantNumber { get; set; }
    public string ConversationId { get; set; }
}
