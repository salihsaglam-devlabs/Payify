using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public interface IMerchantPoolHttpClient
{
    Task<MerchantPoolDto> GetByIdAsync(Guid id);
    Task<PaginatedList<MerchantPoolDto>> GetFilterListAsync(GetFilterMerchantPoolRequest request);
    Task SaveAsync(SaveMerchantPoolRequest request);
    Task<ApproveMerchantPoolResponse> ApproveAsync(ApproveMerchantPoolServiceRequest request);
}
