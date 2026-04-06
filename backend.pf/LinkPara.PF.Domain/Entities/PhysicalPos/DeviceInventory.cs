using LinkPara.PF.Domain.Enums.PhysicalPos;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities.PhysicalPos;

public class DeviceInventory : AuditEntity, ITrackChange
{
    //public string BrandCode { get; set; } //Vendor a gore esitle
    public string SerialNo { get; set; }
    public ContactlessSeparator ContactlessSeparator { get; set; }
    public PhysicalPosVendor PhysicalPosVendor { get; set; }
    public DeviceModel DeviceModel { get; set; }
    public DeviceStatus DeviceStatus { get; set; }
    public DeviceType DeviceType { get; set; }    
    public InventoryType InventoryType { get; set; }
    public List<MerchantPhysicalDevice> MerchantPhysicalDevices { get; set; }
    public List<DeviceInventoryHistory> DeviceInventoryHistoryList { get; set; }
}
