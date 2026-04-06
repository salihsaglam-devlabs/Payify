namespace LinkPara.ApiGateway.CorporateWallet.Services.Notification.Models.Requests;

public class VerifyOtpRequest
{
    public string OtpAuthorizationId { get; set; }
    public string TimeStamp { get; set; }
    public int Code { get; set; }
}