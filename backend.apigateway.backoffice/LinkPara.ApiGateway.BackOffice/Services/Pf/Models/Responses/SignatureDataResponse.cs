namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

public class SignatureDataResponse
{
    public string PublicKey { get; set; }
    public string Nonce { get; set; }
    public string Signature { get; set; }
    public string ConversationId { get; set; }
    public Guid MerchantId { get; set; }
}
