namespace LinkPara.Emoney.ApiGateway.Models.Responses;

public class ApiKeyDto
{
    public string PartnerId { get; set; }
    public PartnerDto Partner { get; set; }
    public string PublicKey { get; set; }
    public string PrivateKey { get; set; }
}
