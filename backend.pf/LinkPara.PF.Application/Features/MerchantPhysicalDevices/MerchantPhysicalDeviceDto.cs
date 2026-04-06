using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Application.Features.DeviceInventories;
using LinkPara.PF.Application.Features.TimeoutTransactions;
using LinkPara.PF.Domain.Entities.PhysicalPos;
using LinkPara.PF.Domain.Enums.PhysicalPos;

namespace LinkPara.PF.Application.Features.MerchantPhysicalDevices;

public class MerchantPhysicalDeviceDto : IMapFrom<MerchantPhysicalDevice>
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
