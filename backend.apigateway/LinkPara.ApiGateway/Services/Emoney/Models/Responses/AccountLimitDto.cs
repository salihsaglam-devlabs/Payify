using LinkPara.ApiGateway.Services.Emoney.Models.Enums;

namespace LinkPara.ApiGateway.Services.Emoney.Models.Responses;

public class AccountLimitDto
{
    public Guid AccountId { get; set; }
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