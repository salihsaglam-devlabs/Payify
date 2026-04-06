using LinkPara.ApiGateway.CorporateWallet.Services.Identity.Models.Enums;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Identity.Models.Responses;

public class RoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public RoleScope RoleScope { get; set; }
}
