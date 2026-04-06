using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Application.Features.TimeoutTransactions;
using LinkPara.PF.Domain.Entities.PhysicalPos;
using LinkPara.PF.Domain.Enums.PhysicalPos;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Application.Features.DeviceInventories;

public class DeviceInventoryDto : IMapFrom<DeviceInventory>
{
    public Guid Id { get; set; }
    public string SerialNo { get; set; }
    public ContactlessSeparator ContactlessSeparator { get; set; }
    public PhysicalPosVendor PhysicalPosVendor { get; set; }
    public DeviceModel DeviceModel { get; set; }
    public DeviceStatus DeviceStatus { get; set; }
    public DeviceType DeviceType { get; set; }
    public InventoryType InventoryType { get; set; }
    public DateTime CreateDate { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public TransactionMerchantResponse Merchant { get; set; }
}
