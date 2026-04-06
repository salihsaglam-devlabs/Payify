namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

public class MerchantApiKeyDto
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public string MerchantNumber { get; set; }
    public string PublicKey { get; set; }
    public string PrivateKey { get; set; }
    public string PrivateKeyEncrypted { get; set; }
}
