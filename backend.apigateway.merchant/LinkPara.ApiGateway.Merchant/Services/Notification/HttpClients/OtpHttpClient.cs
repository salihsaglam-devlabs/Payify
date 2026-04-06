using LinkPara.ApiGateway.Merchant.Services.Notification.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Notification.Models.Responses;

namespace LinkPara.ApiGateway.Merchant.Services.Notification.HttpClients;

public class OtpHttpClient : HttpClientBase, IOtpHttpClient
{
    public OtpHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
    : base(client, httpContextAccessor)
    {
    }

    public async Task<SendOtpResponse> SendOtpAsync(SendOtpRequest request)
    {
        var response = await PostAsJsonAsync("v1/Otp", request);

        return await response.Content.ReadFromJsonAsync<SendOtpResponse>();
    }

    public async Task<VerifyOtpResponse> VerifyOtpAsync(VerifyOtpRequest request)
    {
        var response = await PostAsJsonAsync("v1/Otp/verify", request);

        return await response.Content.ReadFromJsonAsync<VerifyOtpResponse>();
    }
}
