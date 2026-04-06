using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.LimitModels;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;

namespace LinkPara.Emoney.Infrastructure.Services.Limit;

public class WithdrawLimitCheck : ILimitCheck
{
    public Task<LimitControlResponse> CheckLimitAsync(LimitControlRequest request, TierLevel tierLevel, 
        AccountCurrentLevel accountCurrentLevel)
    {
        if (!tierLevel.MaxWithdrawalLimitEnabled)
        {
            return Task.FromResult(new LimitControlResponse
                { IsLimitExceeded = false, LimitOperationType = LimitOperationType.Withdrawal });
        }
        
        var isExceeded = tierLevel.DailyMaxWithdrawalAmount < accountCurrentLevel.DailyWithdrawalAmount + request.Amount
                         || tierLevel.MonthlyMaxWithdrawalAmount < accountCurrentLevel.MonthlyWithdrawalAmount + request.Amount
                         || tierLevel.DailyMaxWithdrawalCount <= accountCurrentLevel.DailyWithdrawalCount
                         || tierLevel.MonthlyMaxWithdrawalCount <= accountCurrentLevel.MonthlyWithdrawalCount;
        
        return Task.FromResult(new LimitControlResponse
            { IsLimitExceeded = isExceeded, LimitOperationType = LimitOperationType.Withdrawal });
    }
}