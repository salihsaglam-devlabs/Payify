namespace LinkPara.PF.Application.Features.MerchantDevices.Queries;

public class MerchantDeviceApiKeyDto
{
    public Guid MerchantId { get; set; }
    public string MerchantNumber { get; set; }
    public string SerialNumber { get; set; }
    public string PublicKey { get; set; }
    public string PrivateKeyEncrypted { get; set; }
}