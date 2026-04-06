using LinkPara.IWallet.ApiGateway.Models.Requests;
using LinkPara.IWallet.ApiGateway.Models.Responses;

namespace LinkPara.IWallet.ApiGateway.Services.HttpClients.CampaignManagement;

public class ChargeHttpClient : HttpClientBase, IChargeHttpClient
{
    public ChargeHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
    {
    }

    public async Task<BaseServiceResponse<ChargeResponse>> ChargeAsync(ChargeRequest request)
    {
        var response = await PostAsJsonAsync("v1/IWalletCharge", request);
        var serviceResponse = new BaseServiceResponse<ChargeResponse>(new BaseResponse { IsSuccess = true });

        if (!response.IsSuccessStatusCode)
        {
            var baseResponse = await HandleExceptionAsync(response);

            serviceResponse.IsSuccess = baseResponse.IsSuccess;
            serviceResponse.ErrorCode = baseResponse.ErrorCode;
            serviceResponse.Message = baseResponse.Message;
            return serviceResponse;
        }

        var processGuid = await response.Content.ReadFromJsonAsync<Guid>();
        serviceResponse.Data = new ChargeResponse { ProcessGuid = processGuid };
        return serviceResponse;
    }
}
