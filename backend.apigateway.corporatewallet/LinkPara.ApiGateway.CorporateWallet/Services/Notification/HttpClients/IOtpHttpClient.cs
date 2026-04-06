using LinkPara.ApiGateway.CorporateWallet.Services.Notification.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Notification.Models.Responses;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Notification.HttpClients
{
    public interface IOtpHttpClient
    {
        Task<SendOtpResponse> SendOtpAsync(SendOtpRequest request);

        Task<VerifyOtpResponse> VerifyOtpAsync(VerifyOtpRequest request);
    }
}
