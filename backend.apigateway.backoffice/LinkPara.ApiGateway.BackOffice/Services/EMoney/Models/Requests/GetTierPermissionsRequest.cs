using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;

public class GetTierPermissionsRequest
{
    public TierLevelType? TierLevel { get; set; }
    public TierPermissionType? PermissionType { get; set; }
    public bool? IsEnabled { get; set; }
}