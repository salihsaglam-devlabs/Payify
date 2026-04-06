using LinkPara.HttpProviders.Emoney.Enums;

namespace LinkPara.HttpProviders.Emoney.Models;

public class CheckOrUpgradeAccountTierRequest
{
    public Guid AccountId { get; set; }
    public AccountTierValidation ValidationType { get; set; }
}