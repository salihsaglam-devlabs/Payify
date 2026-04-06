using LinkPara.HttpProviders.Emoney.Enums;
using TierLevelType = LinkPara.ApiGateway.Boa.Services.Emoney.Models.Enums.TierLevelType;

namespace LinkPara.ApiGateway.Boa.Services.Emoney.Models.Responses;

public class TierLevelUpgradePathDto
{
    public Guid Id { get; set; }
    public TierLevelType TierLevel { get; set; }
    public AccountTierValidation ValidationType { get; set; }
    public TierLevelType NextTier { get; set; }
}