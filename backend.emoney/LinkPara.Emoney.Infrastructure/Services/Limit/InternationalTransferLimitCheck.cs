using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.LimitModels;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;

namespace LinkPara.Emoney.Infrastructure.Services.Limit;

public class InternationalTransferLimitCheck : ILimitCheck
{
    public Task<LimitControlResponse> CheckLimitAsync(LimitControlRequest request, TierLevel tierLevel,
        AccountCurrentLevel accountCurrentLevel)
    {
        if (!tierLevel.MaxInternationalTransferLimitEnabled)
        {
            return Task.FromResult(new LimitControlResponse
            {
                IsLimitExceeded = false,
                LimitOperationType = LimitOperationType.InternationalTransfer
            });
        }
        
        var isExceeded = tierLevel.DailyMaxInternationalTransferAmount < accountCurrentLevel.DailyInternationalTransferAmount + request.Amount
                         || tierLevel.MonthlyMaxInternationalTransferAmount < accountCurrentLevel.MonthlyInternationalTransferAmount + request.Amount
                         || tierLevel.DailyMaxInternationalTransferCount <= accountCurrentLevel.DailyInternationalTransferCount 
                         || tierLevel.MonthlyMaxInternationalTransferCount <= accountCurrentLevel.MonthlyInternationalTransferCount;

        return Task.FromResult(new LimitControlResponse
        {
            IsLimitExceeded = isExceeded,
            LimitOperationType = LimitOperationType.InternationalTransfer
        });
    }
}