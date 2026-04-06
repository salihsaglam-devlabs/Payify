using LinkPara.Emoney.Application.Features.WithdrawRequests;
using LinkPara.Emoney.Application.Features.WithdrawRequests.Queries.GetWithdrawRequestList;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.Emoney.Application.Commons.Interfaces;

public interface IWithdrawRequestService
{
    Task<PaginatedList<WithdrawRequestAdminDto>> GetWithdrawRequestListAsync(GetWithdrawRequestListQuery request);
}
