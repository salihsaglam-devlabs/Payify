using LinkPara.PF.Application.Commons.Models.Limit;
using LinkPara.PF.Application.Commons.Models.Payments.Response;
using LinkPara.PF.Application.Features.Payments.Commands.Provision;
using LinkPara.PF.Domain.Entities;

namespace LinkPara.PF.Application.Commons.Interfaces
{
    public interface ILimitService
    {
        Task<ValidationResponse> CheckLimitAsync(CheckLimitRequest request);
        Task IncrementMerchantDailyUsageAsync(MerchantTransaction merchantTransaction);
        Task IncrementMerchantMonthlyUsageAsync(MerchantTransaction merchantTransaction);
        Task IncrementSubMerchantMonthlyUsageAsync(MerchantTransaction merchantTransaction);
        Task IncrementSubMerchantDailyUsageAsync(MerchantTransaction merchantTransaction);
        Task DecrementMerchantDailyUsageAsync(DecreaseMerchantLimitRequest request);
        Task DecrementMerchantMonthlyUsageAsync(DecreaseMerchantLimitRequest request);
        Task DecrementSubMerchantMonthlyUsageAsync(DecreaseMerchantLimitRequest request);
        Task DecrementSubMerchantDailyUsageAsync(DecreaseMerchantLimitRequest request);
    }
}
