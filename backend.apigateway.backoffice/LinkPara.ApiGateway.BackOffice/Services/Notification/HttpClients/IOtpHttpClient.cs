using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Responses;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.HttpClients
{
    public interface IOtpHttpClient
    {
        Task<SendOtpResponse> SendOtpAsync(SendOtpRequest request);

        Task<VerifyOtpResponse> VerifyOtpAsync(VerifyOtpRequest request);
    }
}
