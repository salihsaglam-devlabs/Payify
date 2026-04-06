using LinkPara.ApiGateway.Merchant.Services.Notification.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Notification.Models.Responses;

namespace LinkPara.ApiGateway.Merchant.Services.Notification.HttpClients
{
    public interface IOtpHttpClient
    {
        Task<SendOtpResponse> SendOtpAsync(SendOtpRequest request);

        Task<VerifyOtpResponse> VerifyOtpAsync(VerifyOtpRequest request);
    }
}
