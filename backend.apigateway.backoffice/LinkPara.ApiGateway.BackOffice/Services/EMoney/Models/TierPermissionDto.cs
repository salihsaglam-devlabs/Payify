using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models;

public class TierPermissionDto
{
    public Guid Id { get; set; }
    public TierLevelType TierLevel { get; set; }
    public TierPermissionType PermissionType { get; set; }
    public bool IsEnabled { get; set; }
}