using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities.PhysicalPos;

public class MerchantDeviceApiKey : AuditEntity, ITrackChange
{
    public string PublicKey { get; set; }
    public string PrivateKeyEncrypted { get; set; }

    public Guid MerchantPhysicalDeviceId { get; set; }
    public MerchantPhysicalDevice MerchantPhysicalDevice { get; set; }
}
