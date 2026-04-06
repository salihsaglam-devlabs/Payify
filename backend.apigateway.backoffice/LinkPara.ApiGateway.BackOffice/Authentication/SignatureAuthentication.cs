namespace LinkPara.ApiGateway.BackOffice.Authentication;

public class SignatureAuthentication
{
    public string PublicKey { get; set; }
    public string Nonce { get; set; }
    public string Signature { get; set; }
    public string ConversationId { get; set; }
}
