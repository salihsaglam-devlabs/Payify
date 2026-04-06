using LinkPara.Emoney.Application.Commons.Models.LimitModels;
using LinkPara.Emoney.Application.Features.Limits;
using LinkPara.Emoney.Application.Features.Limits.Queries.GetAccountCurrentLimits;
using LinkPara.Emoney.Domain.Entities;

namespace LinkPara.Emoney.Application.Commons.Interfaces;

public interface ILimitService
{ 
    Task<LimitControlResponse> IsLimitExceededAsync(LimitControlRequest request);
    Task IncreaseUsageAsync(AccountLimitUpdateRequest request, AccountCurrentLevel currentLevel);
    Task DecreaseUsageAsync(AccountLimitUpdateRequest request, AccountCurrentLevel currentLevel);
    Task<AccountLimitDto> GetAccountLimitsQuery(GetAccountLimitsQuery request);
}