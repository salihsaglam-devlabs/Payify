using LinkPara.HttpProviders.Emoney.Enums;

namespace LinkPara.Emoney.Application.Features.Limits;

public class UserLimitDto
{
    public Guid UserId { get; set; }
    public TierLevelType TierLevelType { get; set; }
    public string CurrencySymbol { get; set; }
    public BalanceLimitDto IndividualWallet { get; set; }
    public UsageLimitDto InternalTransfer { get; set; }
    public UsageLimitDto InternationalTransfer { get; set; }
    public UsageLimitDto Deposit { get; set; }
    public UsageLimitDto Withdraw { get; set; }
    public IbanLimitDto WithdrawToOwnIban { get; set; }
    public IbanLimitDto WithdrawToOtherIban { get; set; }
    public List<TierPermissionDto> TierPermissions { get; set; }
    public List<TierLevelUpgradePathDto> TierLevelUpgradePaths { get; set; }
    public string Name { get; set; }
}
