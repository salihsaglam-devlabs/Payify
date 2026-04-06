using LinkPara.ApiGateway.OpenBanking.Services.Emoney.Responses;
using LinkPara.ApiGateway.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Services.Emoney.Models.Responses;

namespace LinkPara.ApiGateway.Services.Emoney.HttpClients
{
    public interface IConsentOperationHttpClient
    {
        Task<List<ConsentDto>> GetActiveConsentListAsync(GetActiveConsentListRequest request);
        Task<CancelConsentResultDto> CancelConsentAsync(CancelConsentRequest request);
        Task<GetWaitingApprovalConsentResponse> GetWaitingApprovalConsentsAsync(GetWaitingApprovalConsentRequest request);
        Task<GetConsentDetailResponse> GetConsentDetailAsync(GetConsentDetailRequest request);
        Task<UpdateConsentResultDto> UpdateConsentAsync(UpdateConsentRequest request);

    }
}