using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;

public class MerchantPreApplicationHttpClient : HttpClientBase, IMerchantPreApplicationHttpClient
{
    public MerchantPreApplicationHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<MerchantPreApplicationResponse>  CreatePosApplicationAsync(CreateMerchantPreApplicationRequest request)
    {
        var response  = await PostAsJsonAsync($"v1/MerchantPreApplication", request);
        var merchantResponse = await response.Content.ReadFromJsonAsync<MerchantPreApplicationResponse>();
        return merchantResponse ?? throw new InvalidOperationException();
    }
}