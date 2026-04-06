using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients
{
    public interface ILinkHttpClient
    {
        Task<LinkResponse> SaveAsync(SaveLinkRequest request);
        Task<LinkRequirementResponse> GetCreateLinkRequirements(Guid merchantId);
        Task DeleteLinkAsync(Guid id);
        Task<PaginatedList<LinkDto>> GetAllAsync(GetFilterLinkRequest request);
        Task<PaginatedList<LinkPaymentDetailResponse>> GetLinkPaymentDetailAsync(GetPaymentDetailRequest request);
    }
}
