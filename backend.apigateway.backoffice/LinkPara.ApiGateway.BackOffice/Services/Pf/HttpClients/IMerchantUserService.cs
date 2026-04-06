using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public interface IMerchantUserService
{
    Task<PaginatedList<MerchantUserDto>> GetAllAsync(GetAllMerchantUserRequest request);
    Task<MerchantUserDto> GetByIdAsync(Guid id);
    Task SaveAsync(SaveMerchantUserRequest request);
    Task UpdateAsync(MerchantUserDto request);
}
