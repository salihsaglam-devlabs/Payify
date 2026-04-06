namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class SignatureDataRequest
{
    public string PublicKey { get; set; }
    public string MerchantNumber { get; set; }
    public string ConversationId { get; set; }
}
