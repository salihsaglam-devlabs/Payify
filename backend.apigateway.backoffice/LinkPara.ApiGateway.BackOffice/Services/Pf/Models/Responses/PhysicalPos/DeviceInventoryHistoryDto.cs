using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums.PhysicalPos;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses.PhysicalPos;

public class DeviceInventoryHistoryDto
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
