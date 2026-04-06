using LinkPara.PF.Domain.Enums.PhysicalPos;

namespace LinkPara.PF.Application.Commons.Models.MerchantPhysicalDevices;

public class SaveMerchantPhysicalDeviceRequest
{
    public string OwnerPspNo { get; set; }
    public bool IsPinPad { get; set; }
    public ConnectionType ConnectionType { get; set; }
    public AssignmentType AssignmentType { get; set; }
    public string FiscalNo { get; set; }
    public Guid DeviceInventoryId { get; set; }
}
