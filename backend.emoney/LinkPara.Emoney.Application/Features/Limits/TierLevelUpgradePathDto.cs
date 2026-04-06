using LinkPara.Emoney.Application.Commons.Mappings;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.HttpProviders.Emoney.Enums;

namespace LinkPara.Emoney.Application.Features.Limits;

public class TierLevelUpgradePathDto : IMapFrom<TierLevelUpgradePath>
{
    public Guid Id { get; set; }
    public TierLevelType TierLevel { get; set; }
    public AccountTierValidation ValidationType { get; set; }
    public TierLevelType NextTier { get; set; }
}