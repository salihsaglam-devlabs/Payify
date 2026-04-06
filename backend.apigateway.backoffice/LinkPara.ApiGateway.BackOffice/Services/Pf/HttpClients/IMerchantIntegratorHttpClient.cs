using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public interface IMerchantIntegratorHttpClient
{
    Task<PaginatedList<MerchantIntegratorDto>> GetAllAsync(SearchQueryParams request);
    Task<MerchantIntegratorDto> GetByIdAsync(Guid id);
    Task SaveAsync(SaveMerchantIntegratorRequest request);
    Task UpdateAsync(UpdateMerchantIntegratorRequest request);
    Task DeleteMerchantIntegratorAsync(Guid id);
}
