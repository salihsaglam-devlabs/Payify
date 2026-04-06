using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums.PhysicalPos;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests.PhysicalPos;

public class GetAllMerchantPhysicalDeviceRequest : SearchQueryParams
{
    public Guid? MerchantId { get; set; }
    public DeviceType? DeviceType { get; set; }
    public PhysicalPosVendor? PhysicalPosVendor { get; set; }
    public DeviceModel? DeviceModel { get; set; }
    public string SerialNo { get; set; }
    public DeviceStatus? DeviceStatus { get; set; }
    public ContactlessSeparator? ContactlessSeparator { get; set; }
    public string FiscalNo { get; set; }
    public RecordStatus? RecordStatus { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
}
