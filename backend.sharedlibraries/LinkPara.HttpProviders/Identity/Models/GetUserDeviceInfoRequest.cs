using LinkPara.HttpProviders.Identity.Models.Enums;

namespace LinkPara.HttpProviders.Identity.Models;

public class GetUserDeviceInfoRequest
{
    public List<Guid> UserIdList { get; set; }
}