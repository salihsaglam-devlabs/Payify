using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Application.Features.DeviceInventories;
using LinkPara.PF.Domain.Entities.PhysicalPos;
using LinkPara.PF.Domain.Enums.PhysicalPos;

namespace LinkPara.PF.Application.Features.DeviceInventoryHistories;

public class DeviceInventoryHistoryDto : IMapFrom<DeviceInventoryHistory>
{
    public Guid Id { get; set; }
    public Guid DeviceInventoryId { get; set; }
    public DeviceHistoryType DeviceHistoryType { get; set; }
    public string NewData { get; set; }
    public string OldData { get; set; }
    public string Detail { get; set; }
    public string CreatedNameBy { get; set; }
    public DateTime CreateDate { get; set; }
    public DeviceInventoryDto DeviceInventory { get; set; }
}
