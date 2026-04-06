namespace LinkPara.ApiGateway.BackOffice.Authentication;

public class SignatureValidationParameters
{
    public string PrivateKey { get; set; }
    public string PublicKey { get; set; }
    public string Nonce { get; set; }
    public string ConversationId { get; set; }
    public string Signature { get; set; }
}
