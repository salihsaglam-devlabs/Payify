using LinkPara.SharedModels.Authorization.Enums;

namespace LinkPara.Identity.Application.Common.Models.AuthorizationModels;

public class ModuleClaim
{
    public string Module { get; set; }
    public List<PermissionOperationType> PermissionOperationTypes { get; set; }
}