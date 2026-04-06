using LinkPara.IWallet.ApiGateway.Models.Requests;
using LinkPara.IWallet.ApiGateway.Models.Responses;

namespace LinkPara.IWallet.ApiGateway.Services.HttpClients.CampaignManagement
{
    public interface IReverseChargeHttpClient
    {
        Task<BaseServiceResponse<ReverseChargeResponse>> ReverseChargeAsync(ReverseChargeRequest request);
    }
}
