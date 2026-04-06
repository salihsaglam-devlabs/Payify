using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums.PhysicalPos;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses.PhysicalPos;

public class MerchantPhysicalDeviceDto
{
    public Guid Id { get; set; }
    public string OwnerPspNo { get; set; }
    public bool IsPinPad { get; set; }
    public ConnectionType ConnectionType { get; set; }
    public AssignmentType AssignmentType { get; set; }
    public string FiscalNo { get; set; }
    public string OwnerTerminalId { get; set; }
    public Guid MerchantId { get; set; }
    public TransactionMerchantResponse Merchant { get; set; }
    public Guid DeviceInventoryId { get; set; }
    public DeviceInventoryDto DeviceInventory { get; set; }
    public List<MerchantPhysicalPosDto> MerchantPhysicalPosList { get; set; }
}
