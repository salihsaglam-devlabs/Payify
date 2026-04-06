namespace LinkPara.Emoney.ApiGateway.Authentication;

public class PrivateKeyValidationParameters
{
    public string PrivateKey { get; set; }
    public string PublicKey { get; set; }
    public string Nonce { get; set; }
    public string Signature { get; set; }
}
