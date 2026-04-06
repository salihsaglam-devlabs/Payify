using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;

public class SignatureDataRequest
{
    public string PublicKey { get; set; }
    public string MerchantNumber { get; set; }
    public string ConversationId { get; set; }
    public Guid? SubMerchantId { get; set; }
}
