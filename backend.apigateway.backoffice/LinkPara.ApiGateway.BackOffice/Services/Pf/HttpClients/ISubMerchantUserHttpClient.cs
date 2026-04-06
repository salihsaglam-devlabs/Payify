using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public interface ISubMerchantUserHttpClient
{
    Task<PaginatedList<SubMerchantUserDto>> GetAllAsync(GetAllSubMerchantUserRequest request);
    Task<SubMerchantUserDto> GetByIdAsync(Guid id);
    Task SaveAsync(SaveSubMerchantUserRequest request);
    Task UpdateAsync(SubMerchantUserDto request);
}