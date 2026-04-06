using LinkPara.Emoney.Application.Features.Limits;
using LinkPara.Emoney.Application.Features.Limits.Queries.GetTierLevels;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.HttpProviders.Emoney.Enums;

namespace LinkPara.Emoney.Application.Commons.Interfaces;

public interface ITierLevelService
{
    Task<List<TierLevelDto>> GetTierLevelsQueryAsync(GetTierLevelsQuery request);
    Task<AccountCurrentLevel> FindAccountCurrentLevel(Guid accountId, string currencyCode);
    Task<AccountCurrentLevel> PopulateInitialLevelAsync(string currencyCode, Guid accountId, Guid createdBy);
    Task<TierLevel> FindTierLevelAsync(Guid accountId);
    Task<List<TierLevelUpgradePathDto>> GetTierLevelUpgradePathsAsync(TierLevelType tierLevelType);
    Task CheckOrUpgradeAccountTierAsync(Account account, AccountTierValidation validationType);
}