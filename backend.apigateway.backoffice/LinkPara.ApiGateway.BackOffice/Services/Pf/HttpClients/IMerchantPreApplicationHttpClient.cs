using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public interface IMerchantPreApplicationHttpClient
{
    Task<MerchantPreApplicationDto> GetByIdAsync(Guid id);
    Task<PaginatedList<MerchantPreApplicationDto>> GetAllAsync(GetAllMerchantPreApplicationsRequest request);
    Task UpdateAsync(UpdateMerchantPreApplicationRequest request);
    Task DeleteMerchantApplicationAsync(Guid id);
}