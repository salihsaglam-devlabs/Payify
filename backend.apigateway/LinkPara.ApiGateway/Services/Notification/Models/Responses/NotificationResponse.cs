namespace LinkPara.ApiGateway.Services.Notification.Models.Responses
{
    public class NotificationResponse
    {
        public bool Success { get; set; }
        public string ResponseCode { get; set; } = string.Empty;
        public string ResponseMessage { get; set; } = string.Empty;
    }
}
