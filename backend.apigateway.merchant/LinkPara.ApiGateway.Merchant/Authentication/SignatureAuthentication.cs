namespace LinkPara.ApiGateway.Merchant.Authentication;

public class SignatureAuthentication
{
    public string PublicKey { get; set; }
    public string Nonce { get; set; }
    public string Signature { get; set; }
    public string ConversationId { get; set; }
}
