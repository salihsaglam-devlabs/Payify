using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;

public interface IAcquireBankHttpClient
{
    Task<PaginatedList<AcquireBankDto>> GetAllAsync(GetFilterAcquireBankRequest request);
    Task<AcquireBankDto> GetByIdAsync(Guid id);
}
