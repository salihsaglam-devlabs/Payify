using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.LimitModels;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;

namespace LinkPara.Emoney.Infrastructure.Services.Limit;

public class OnUsPaymentLimitCheck : ILimitCheck
{
    public OnUsPaymentLimitCheck()
    {
        
    }

    public async Task<LimitControlResponse> CheckLimitAsync(LimitControlRequest request, TierLevel tierLevel,
        AccountCurrentLevel accountCurrentLevel)
    {
        if (!tierLevel.MaxOnUsPaymentLimitEnabled)
        {
            return new LimitControlResponse
                { IsLimitExceeded = false, LimitOperationType = LimitOperationType.OnUs };
        }

        var isLimitExceeded = tierLevel.DailyMaxOnUsPaymentAmount < accountCurrentLevel.DailyOnUsPaymentAmount + request.Amount
                              || tierLevel.MonthlyMaxOnUsPaymentAmount < accountCurrentLevel.MonthlyOnUsPaymentAmount + request.Amount
                              || tierLevel.DailyMaxOnUsPaymentCount <= accountCurrentLevel.DailyOnUsPaymentCount
                              || tierLevel.MonthlyMaxOnUsPaymentCount <= accountCurrentLevel.MonthlyOnUsPaymentCount;

        return new LimitControlResponse
            { IsLimitExceeded = isLimitExceeded, LimitOperationType = LimitOperationType.OnUs };
    }
}