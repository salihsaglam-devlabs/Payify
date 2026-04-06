using LinkPara.ApiGateway.Services.Notification.Models.Requests;
using LinkPara.ApiGateway.Services.Notification.Models.Responses;

namespace LinkPara.ApiGateway.Services.Notification.HttpClients
{
    public interface IOtpHttpClient
    {
        Task<SendOtpResponse> SendOtpAsync(SendOtpRequest request);

        Task<VerifyOtpResponse> VerifyOtpAsync(VerifyOtpRequest request);
    }
}
