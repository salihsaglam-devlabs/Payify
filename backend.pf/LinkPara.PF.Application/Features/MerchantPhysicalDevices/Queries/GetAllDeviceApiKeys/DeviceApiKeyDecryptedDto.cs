namespace LinkPara.PF.Application.Features.MerchantPhysicalDevices.Queries.GetAllDeviceApiKeys;

public class DeviceApiKeyDecryptedDto
{
    public Guid MerchantId { get; set; }
    public string MerchantNumber { get; set; }
    public string SerialNumber { get; set; }
    public string PublicKey { get; set; }
    public string PrivateKeyEncrypted { get; set; }
    public string PrivateKey { get; set; }
}