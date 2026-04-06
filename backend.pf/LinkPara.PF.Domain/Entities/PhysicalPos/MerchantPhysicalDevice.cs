using LinkPara.PF.Domain.Enums.PhysicalPos;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities.PhysicalPos;

public class MerchantPhysicalDevice : AuditEntity, ITrackChange
{
    public string OwnerPspNo { get; set; } 
    public bool IsPinPad { get; set; }
    public ConnectionType ConnectionType { get; set; }
    public AssignmentType AssignmentType { get; set; } // ownerpspno buna gore gidecek
    public string FiscalNo { get; set; }
    public string OwnerTerminalId { get; set; }
    public Guid MerchantId { get; set; }
    public Merchant Merchant { get; set; }
    public Guid DeviceInventoryId { get; set; }
    public DeviceInventory DeviceInventory { get; set; }
    public List<MerchantPhysicalPos> MerchantPhysicalPosList { get; set; }
    public List<MerchantDeviceApiKey> DeviceApiKeys { get; set; }
}
