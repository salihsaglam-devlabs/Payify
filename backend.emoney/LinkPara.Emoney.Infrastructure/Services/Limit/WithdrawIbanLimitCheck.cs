using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.LimitModels;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;

namespace LinkPara.Emoney.Infrastructure.Services.Limit;

public class WithdrawIbanLimitCheck : ILimitCheck
{
    public Task<LimitControlResponse> CheckLimitAsync(LimitControlRequest request, TierLevel tierLevel, 
        AccountCurrentLevel accountCurrentLevel)
    {
        var isOwnIban = request.IsOwnIban ?? false;
        var isDailyDistinctIban = request.IsDailyDistinctIban ?? false;
        var isMonthlyDistinctIban = request.IsMonthlyDistinctIban ?? false;
        var isExceeded = false;
        
        if (isOwnIban)
        {
            if (tierLevel.MaxOwnIbanWithdrawalLimitEnabled)
            {
                isExceeded =
                    tierLevel.DailyMaxOwnIbanWithdrawalCount <= accountCurrentLevel.DailyOwnIbanWithdrawalCount ||
                    tierLevel.MonthlyMaxOwnIbanWithdrawalCount <= accountCurrentLevel.MonthlyOwnIbanWithdrawalCount;
            }
        }
        else
        {
            if (tierLevel.MaxOtherIbanWithdrawalLimitEnabled)
            {
                isExceeded =
                    tierLevel.DailyMaxOtherIbanWithdrawalCount <= accountCurrentLevel.DailyOtherIbanWithdrawalCount ||
                    tierLevel.DailyMaxOtherIbanWithdrawalAmount < accountCurrentLevel.DailyOtherIbanWithdrawalAmount + request.Amount ||
                    (isDailyDistinctIban && tierLevel.DailyMaxDistinctOtherIbanWithdrawalCount <= accountCurrentLevel.DailyDistinctOtherIbanWithdrawalCount) ||
                    tierLevel.MonthlyMaxOtherIbanWithdrawalCount <= accountCurrentLevel.MonthlyOtherIbanWithdrawalCount ||
                    tierLevel.MonthlyMaxOtherIbanWithdrawalAmount < accountCurrentLevel.MonthlyOtherIbanWithdrawalAmount + request.Amount ||
                    (isMonthlyDistinctIban && tierLevel.MonthlyMaxDistinctOtherIbanWithdrawalCount <= accountCurrentLevel.MonthlyDistinctOtherIbanWithdrawalCount);
            }
        }
        
        return Task.FromResult(new LimitControlResponse
            { IsLimitExceeded = isExceeded, LimitOperationType = LimitOperationType.WithdrawIban });
    }
}