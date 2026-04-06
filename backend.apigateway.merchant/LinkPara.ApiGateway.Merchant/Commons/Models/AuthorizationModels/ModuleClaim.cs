using LinkPara.SharedModels.Authorization.Enums;

namespace LinkPara.ApiGateway.Merchant.Commons.Models.AuthorizationModels;

public class ModuleClaim
{
    public string Module { get; set; }
    public List<PermissionOperationType> PermissionOperationTypes { get; set; }
}