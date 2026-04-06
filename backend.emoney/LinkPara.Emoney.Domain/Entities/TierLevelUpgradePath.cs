using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Emoney.Domain.Entities;

public class TierLevelUpgradePath : AuditEntity
{
    public TierLevelType TierLevel { get; set; }
    public AccountTierValidation ValidationType { get; set; }
    public TierLevelType NextTier { get; set; }
}