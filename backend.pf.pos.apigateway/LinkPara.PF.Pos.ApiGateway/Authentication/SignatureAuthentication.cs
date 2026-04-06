namespace LinkPara.PF.Pos.ApiGateway.Authentication;

public class SignatureAuthentication
{
    public string PublicKey { get; set; }
    public string Nonce { get; set; }
    public string Signature { get; set; }
    public string ConversationId { get; set; }
    public string SerialNumber { get; set; }
}