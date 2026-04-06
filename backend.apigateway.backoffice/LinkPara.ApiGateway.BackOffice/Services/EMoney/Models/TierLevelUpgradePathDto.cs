using LinkPara.HttpProviders.Emoney.Enums;
using TierLevelType = LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Enums.TierLevelType;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models;

public class TierLevelUpgradePathDto
{
    public Guid Id { get; set; }
    public TierLevelType TierLevel { get; set; }
    public AccountTierValidation ValidationType { get; set; }
    public TierLevelType NextTier { get; set; }
}