using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients
{
    public interface IMerchantBusinessPartnerHttpClient
    {
        Task<PaginatedList<MerchantBusinessPartnerDto>> GetAllAsync(GetAllMerchantBusinessPartnerRequest request);
        Task<MerchantBusinessPartnerDto> GetByIdAsync(Guid id);
        Task SaveAsync(SaveMerchantBusinessPartnerRequest request);
        Task UpdateAsync(MerchantBusinessPartnerDto request);
    }
}
