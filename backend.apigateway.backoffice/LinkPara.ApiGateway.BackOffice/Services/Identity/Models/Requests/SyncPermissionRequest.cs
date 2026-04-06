
using LinkPara.SharedModels.Authorization.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests;

public class SyncPermissionRequest
{
    public string ClaimType { get; set; }
    public string ClaimValue { get; set; }
    public string Module { get; set; }
    public PermissionOperationType OperationType { get; set; }
    public string NormalizedClaimValue { get; set; }
    public string Description { get; set; }
    public string DisplayName { get; set; }
    public bool Display { get; set; }
}