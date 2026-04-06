namespace LinkPara.Emoney.ApiGateway.Models.Responses;

public class SignatureTestDataResponse
{
    public string PublicKey { get; set; }
    public string Nonce { get; set; }
    public string Signature { get; set; }
    public Guid PartnerId { get; set; }
}
