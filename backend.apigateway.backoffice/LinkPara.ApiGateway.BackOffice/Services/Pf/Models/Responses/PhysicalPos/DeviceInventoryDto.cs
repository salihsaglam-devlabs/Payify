using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums.PhysicalPos;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses.PhysicalPos;

public class DeviceInventoryDto
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
