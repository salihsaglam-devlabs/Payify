using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public interface IMerchantBlockageHttpClient
{
    Task<PaginatedList<MerchantBlockageDto>> GetAllAsync(GetFilterMerchantBlockageRequest request);
    Task<MerchantBlockageDto> GetByMerchantIdAsync(Guid merchantId);
    Task SaveAsync(SaveMerchantBlockageRequest request);
    Task UpdateAsync(UpdateMerchantBlockageRequest request);
    Task UpdatePaymentDateAsync(UpdatePaymentDateRequest request);
}
