using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums.PhysicalPos;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests.PhysicalPos;

public class SaveMerchantPhysicalRequest
{
    public string OwnerPspNo { get; set; }
    public bool IsPinPad { get; set; }
    public ConnectionType ConnectionType { get; set; }
    public AssignmentType AssignmentType { get; set; }
    public string FiscalNo { get; set; }
    public Guid DeviceInventoryId { get; set; }
}
