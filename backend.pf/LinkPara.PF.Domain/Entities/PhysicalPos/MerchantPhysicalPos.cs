using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Domain.Enums.PhysicalPos;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities.PhysicalPos;

public class MerchantPhysicalPos : AuditEntity, ITrackChange
{
    public Guid MerchantPhysicalDeviceId { get; set; }
    public MerchantPhysicalDevice MerchantPhysicalDevice { get; set; }
    public Guid PhysicalPosId { get; set; }
    public PhysicalPos PhysicalPos { get; set; }
    public string PosMerchantId { get; set; } //ServiceProviderPspMerchantId
    public string PosTerminalId { get; set; } //TerminalId
    public TerminalStatus TerminalStatus { get; set; }
    public string BkmReferenceNumber { get; set; }
    public DeviceTerminalStatus  DeviceTerminalStatus { get; set; }
    public DateTime DeviceTerminalLastActivity { get; set; } 
}
