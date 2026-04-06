using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums.PhysicalPos;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests.PhysicalPos;

public class UpdateMerchantPhysicalDeviceRequest
{
    public Guid Id { get; set; }
    public DeviceStatus DeviceStatus { get; set; }
}
