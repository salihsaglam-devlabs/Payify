using LinkPara.ApiGateway.Boa.Services.Emoney.Models.Enums;

namespace LinkPara.ApiGateway.Boa.Services.Emoney.Models.Responses;

public class TierPermissionDto
{
    public Guid Id { get; set; }
    public TierLevelType TierLevel { get; set; }
    public TierPermissionType PermissionType { get; set; }
    public bool IsEnabled { get; set; }
}