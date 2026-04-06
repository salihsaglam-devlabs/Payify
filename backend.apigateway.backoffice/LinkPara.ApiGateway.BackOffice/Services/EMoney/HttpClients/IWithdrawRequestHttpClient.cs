using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;

public interface IWithdrawRequestHttpClient
{
    Task<WithdrawRequestDto> GetByIdAsync(Guid id);    
    Task<PaginatedList<WithdrawRequestAdminDto>> GetWithdrawRequestListAsync(GetWithdrawRequestListRequest request);

}
