namespace LinkPara.ApiGateway.Services.Notification.Models.Responses;

public class SendOtpResponse
{
    public string TimeStamp { get; set; }
    public string OtpAuthorizationId { get; set; }
    public DateTime ExpirationDate { get; set; }
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; }
    public string Status { get; set; }
}