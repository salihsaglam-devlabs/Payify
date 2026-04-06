using LinkPara.ApiGateway.BackOffice.Commons.Mappings;

namespace LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Responses;

public class UserWithPermissionsDto : UserDto, IMapFrom<UserDto> 
{
    public List<string> ClaimValues { get; set; }
    public List<string> RoleNames { get; set; }
    public List<string> RoleIds { get; set; }
}