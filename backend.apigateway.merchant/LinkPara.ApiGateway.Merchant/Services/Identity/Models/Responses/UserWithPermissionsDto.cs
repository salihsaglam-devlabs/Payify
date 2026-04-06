using LinkPara.ApiGateway.Merchant.Commons.Mappings;

namespace LinkPara.ApiGateway.Merchant.Services.Identity.Models.Responses;

public class UserWithPermissionsDto : UserDto, IMapFrom<UserDto>
{
    public List<string> ClaimValues { get; set; }
    public List<string> RoleNames { get; set; }
}