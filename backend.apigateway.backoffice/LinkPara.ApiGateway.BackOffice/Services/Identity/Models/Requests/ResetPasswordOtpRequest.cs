namespace LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests;

public class ResetPasswordOtpRequest
{
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
    public string UserName { get; set; }
    public string UserId { get; set; }
    public string TimeStamp { get; set; }
    public string OtpAuthorizationId { get; set; }
    public int Code { get; set; }
}