using LinkPara.IWallet.ApiGateway.Models.Requests;
using LinkPara.IWallet.ApiGateway.Models.Responses;

namespace LinkPara.IWallet.ApiGateway.Services.HttpClients.CampaignManagement;

public class OtpHttpClient : HttpClientBase, IOtpHttpClient
{
    public OtpHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
    {

    }

    public async Task<BaseServiceResponse<SendIWalletSmsOtpResponse>> SendIWalletSmsOtpAsync(SendIWalletSmsOtpRequest request)
    {
        var response = await PostAsJsonAsync("v1/SendIWalletSmsOtp", request);
        var serviceResponse = new BaseServiceResponse<SendIWalletSmsOtpResponse>(new BaseResponse { IsSuccess = true });

        if (!response.IsSuccessStatusCode)
        {
            var baseResponse = await HandleExceptionAsync(response);

            serviceResponse.IsSuccess = baseResponse.IsSuccess;
            serviceResponse.ErrorCode = baseResponse.ErrorCode;
            serviceResponse.Message = baseResponse.Message;
            return serviceResponse;
        }

        var smsResponse = await response.Content.ReadFromJsonAsync<SendIWalletSmsOtpResponse>();
        serviceResponse.Data = smsResponse;
        return serviceResponse;
    }
}
