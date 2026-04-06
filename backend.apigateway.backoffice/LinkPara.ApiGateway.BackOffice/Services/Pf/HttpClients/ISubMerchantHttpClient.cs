using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public interface ISubMerchantHttpClient
{
    Task<PaginatedList<SubMerchantDto>> GetAllAsync(GetAllSubMerchantRequest request);
    Task<SubMerchantDto> GetByIdAsync(Guid id);
}
