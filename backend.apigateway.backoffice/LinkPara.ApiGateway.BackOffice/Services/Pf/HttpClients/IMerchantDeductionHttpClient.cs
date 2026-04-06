using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients
{
    public interface IMerchantDeductionHttpClient
    {
        Task<DeductionDetailsResponse> GetByIdAsync(Guid id);
        Task<PaginatedList<MerchantDeductionDto>> GetAllMerchantDeductionsAsync(GetFilterMerchantDeductionRequest request);
    }
}
