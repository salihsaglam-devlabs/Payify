using LinkPara.IWallet.ApiGateway.Models.Requests;
using LinkPara.IWallet.ApiGateway.Models.Responses;

namespace LinkPara.IWallet.ApiGateway.Services.HttpClients.CampaignManagement;

public class CashBackHttpClient : HttpClientBase, ICashBackHttpClient
{
    public CashBackHttpClient(HttpClient client,IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
    {
    }

    public async Task<BaseServiceResponse<CashBackResponse>> CashBackAsync(CashBackRequest request)
    {
        var response = await PostAsJsonAsync("v1/IWalletCashBack", request);
        var serviceResponse = new BaseServiceResponse<CashBackResponse>(new BaseResponse { IsSuccess = true });

        if (!response.IsSuccessStatusCode)
        {
            var baseResponse = await HandleExceptionAsync(response);

            serviceResponse.IsSuccess = baseResponse.IsSuccess;
            serviceResponse.ErrorCode = baseResponse.ErrorCode;
            serviceResponse.Message = baseResponse.Message;
            return serviceResponse;
        }

        serviceResponse.Data = new CashBackResponse { };
        return serviceResponse;
    }
}
