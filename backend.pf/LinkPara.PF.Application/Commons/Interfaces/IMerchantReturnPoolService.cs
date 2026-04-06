
using LinkPara.PF.Application.Commons.Models.Payments.Response;
using LinkPara.PF.Application.Features.MerchantReturnPools;
using LinkPara.PF.Application.Features.MerchantReturnPools.Queries.GetMerchantReturnPools;
using LinkPara.PF.Application.Features.MerchantTransactions;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.PF.Application.Commons.Interfaces
{
    public interface IMerchantReturnPoolService
    {
        public Task AddAsync(MerchantReturnPoolDto req);
        public Task<MerchantReturnPoolDto> UpdateStatusAsync(Guid merchantReturnPoolId, ReturnStatus returnStatus, ReturnResponse returnResponse, string rejectDescription, string rejectReason);
        public Task<List<MerchantReturnPoolDto>> GetMerchantReturnPoolByOrderIdAsync(string orderId);
        public Task<MerchantReturnPoolDto> GetByIdAsync(Guid id);
        public Task<List<MerchantTransactionDto>> GetMerchantMonthlyReturnTransactionsAsync(Guid merchantId);
        public Task<PaginatedList<MerchantReturnPoolDto>> GetPaginatedPendingPoolAsync(GetMerchantReturnPoolsQuery request);
    }
}
