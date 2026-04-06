using LinkPara.ApiGateway.CorporateWallet.Services.Notification.Models.Enums;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Notification.Models.Requests
{
    public class SendOtpRequest
    {
        public string Receiver { get; set; }
        public OtpType OtpType { get; set; }
        public OtpAction Action { get; set; }
        public string RecaptchaToken { get; set; }
    }
}
