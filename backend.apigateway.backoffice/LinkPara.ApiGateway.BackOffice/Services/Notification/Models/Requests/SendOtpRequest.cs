using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests
{
    public class SendOtpRequest
    {
        public string Receiver { get; set; }
        public string Username { get; set; }
        public OtpType OtpType { get; set; }
        public OtpAction Action { get; set; }
    }
}
