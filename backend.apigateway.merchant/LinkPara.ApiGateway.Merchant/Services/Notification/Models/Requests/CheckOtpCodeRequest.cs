namespace LinkPara.ApiGateway.Merchant.Services.Notification.Models.Requests
{
    public class CheckOtpCodeRequest
    {
        public string UserId { get; set; } = null!;
        public int Code { get; set; }
    }
}
