using LinkPara.SharedModels.Authorization.Enums;

namespace LinkPara.ApiGateway.CorporateWallet.Commons.Models.AuthorizationModels;

public class ModuleClaim
{
    public string Module { get; set; }
    public List<PermissionOperationType> PermissionOperationTypes { get; set; }
}