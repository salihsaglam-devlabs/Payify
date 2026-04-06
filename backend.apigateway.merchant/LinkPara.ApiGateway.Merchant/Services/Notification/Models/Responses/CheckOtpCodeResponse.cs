namespace LinkPara.ApiGateway.Merchant.Services.Notification.Models.Responses
{
    public class CheckOtpCodeResponse
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
