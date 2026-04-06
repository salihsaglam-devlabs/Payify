using LinkPara.ApiGateway.BackOffice.Commons.Models.AuthorizationModels;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests;

public class UpdateRoleRequest
{
    public string RoleId { get; set; }
    public string Name { get; set; }
    public RoleScope RoleScope { get; set; }
}