namespace LinkPara.PF.Pos.ApiGateway.Models.Responses;

public class MerchantDeviceApiKeyDto
{
    public Guid MerchantId { get; set; }
    public string MerchantNumber { get; set; }
    public string SerialNumber { get; set; }
    public string PublicKey { get; set; }
    public string PrivateKeyEncrypted { get; set; }
}