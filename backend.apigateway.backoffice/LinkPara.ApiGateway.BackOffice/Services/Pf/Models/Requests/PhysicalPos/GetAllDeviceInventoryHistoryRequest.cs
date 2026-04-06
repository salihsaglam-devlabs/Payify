using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums.PhysicalPos;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests.PhysicalPos;

public class GetAllDeviceInventoryHistoryRequest : SearchQueryParams
{
    public Guid? DeviceInventoryHistoryId { get; set; }
    public Guid? DeviceInventoryId { get; set; }
    public DeviceModel? DeviceModel { get; set; }
    public DeviceType? DeviceType { get; set; }
    public PhysicalPosVendor? PhysicalPosVendor { get; set; }
}
