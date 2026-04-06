using LinkPara.IWallet.ApiGateway.Models.Requests;
using LinkPara.IWallet.ApiGateway.Models.Responses;

namespace LinkPara.IWallet.ApiGateway.Services.HttpClients.CampaignManagement
{
    public class LoginHttpClient : HttpClientBase, ILoginHttpClient
    {
        public LoginHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
        {
        }

        public async Task<BaseServiceResponse<LoginResponse>> LoginAsync(LoginRequest request)
        {
            var response = await PostAsJsonAsync("v1/IWalletLogin", request);
            var serviceResponse = new BaseServiceResponse<LoginResponse>(new BaseResponse { IsSuccess = true });

            if (!response.IsSuccessStatusCode)
            {
                var baseResponse = await HandleExceptionAsync(response);

                serviceResponse.IsSuccess = baseResponse.IsSuccess;
                serviceResponse.ErrorCode = baseResponse.ErrorCode;
                serviceResponse.Message = baseResponse.Message;
                return serviceResponse;
            }

            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
            serviceResponse.Data = loginResponse;
            return serviceResponse;
        }
    }
}
