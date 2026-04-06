using LinkPara.HttpProviders.Emoney.Models;

namespace LinkPara.HttpProviders.Emoney;

public interface ITierLevelService
{
    Task<List<TierLevelDto>> GetTierLevelsAsync(GetTierLevelsQuery query);
    Task CheckOrUpgradeAccountTierAsync(CheckOrUpgradeAccountTierRequest request);
}