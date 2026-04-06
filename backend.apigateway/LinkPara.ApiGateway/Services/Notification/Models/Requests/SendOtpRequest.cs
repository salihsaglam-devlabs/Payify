using LinkPara.ApiGateway.Services.Notification.Models.Enums;

namespace LinkPara.ApiGateway.Services.Notification.Models.Requests
{
    public class SendOtpRequest
    {
        public string Receiver { get; set; }
        public string Username { get; set; }
        public OtpType OtpType { get; set; }
        public OtpAction Action { get; set; }
        public string RecaptchaToken { get; set; }
    }
}
