using LinkPara.Emoney.Domain.Enums;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Emoney.Domain.Entities;

public class TierPermission : AuditEntity
{
    public TierLevelType TierLevel { get; set; }
    public TierPermissionType PermissionType { get; set; }
    public bool IsEnabled { get; set; }
}