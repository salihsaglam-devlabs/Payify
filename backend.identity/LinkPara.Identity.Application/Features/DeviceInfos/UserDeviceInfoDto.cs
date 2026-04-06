using LinkPara.Identity.Application.Common.Mappings;
using LinkPara.Identity.Domain.Entities;
using UserDeviceInfo = LinkPara.Identity.Domain.Entities.UserDeviceInfo;

namespace LinkPara.Identity.Application.Features.DeviceInfos;

public class UserDeviceInfoDto : IMapFrom<UserDeviceInfo>
{
    public Guid UserId { get; set; }
    public bool IsMainDevice { get; set; }
    public Guid DeviceInfoId { get; set; }
    public DeviceInfoDto DeviceInfo { get; set; }
}