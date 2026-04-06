using LinkPara.PF.Domain.Enums.PhysicalPos;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities.PhysicalPos;

public class DeviceInventoryHistory : AuditEntity
{
    public Guid DeviceInventoryId { get; set; }
    public DeviceHistoryType DeviceHistoryType { get; set; }
    public string NewData { get; set; }
    public string OldData { get; set; } 
    public string Detail { get; set; }
    public string CreatedNameBy { get; set; }
    public DeviceInventory DeviceInventory { get; set; }
}
