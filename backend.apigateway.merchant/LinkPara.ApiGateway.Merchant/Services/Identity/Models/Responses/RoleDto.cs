using LinkPara.ApiGateway.Merchant.Services.Identity.Models.Enums;

namespace LinkPara.ApiGateway.Merchant.Services.Identity.Models.Responses;

public class RoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public RoleScope RoleScope { get; set; }
}
