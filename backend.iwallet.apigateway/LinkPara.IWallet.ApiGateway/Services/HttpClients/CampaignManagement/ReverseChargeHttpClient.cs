using LinkPara.IWallet.ApiGateway.Models.Requests;
using LinkPara.IWallet.ApiGateway.Models.Responses;

namespace LinkPara.IWallet.ApiGateway.Services.HttpClients.CampaignManagement;

public class ReverseChargeHttpClient : HttpClientBase, IReverseChargeHttpClient
{
    public ReverseChargeHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
    {
    }

    public async Task<BaseServiceResponse<ReverseChargeResponse>> ReverseChargeAsync(ReverseChargeRequest request)
    {
        var response = await PostAsJsonAsync("v1/IWalletReverseCharge", request);
        var serviceResponse = new BaseServiceResponse<ReverseChargeResponse>(new BaseResponse { IsSuccess = true });

        if (!response.IsSuccessStatusCode)
        {
            var baseResponse = await HandleExceptionAsync(response);

            serviceResponse.IsSuccess = baseResponse.IsSuccess;
            serviceResponse.ErrorCode = baseResponse.ErrorCode;
            serviceResponse.Message = baseResponse.Message;
            return serviceResponse;
        }

        serviceResponse.Data = new ReverseChargeResponse { };
        return serviceResponse;
    }
}
