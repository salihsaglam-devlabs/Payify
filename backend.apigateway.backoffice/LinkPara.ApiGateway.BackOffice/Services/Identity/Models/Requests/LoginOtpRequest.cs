namespace LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests;

public class LoginOtpRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string TimeStamp { get; set; }
    public string OtpAuthorizationId { get; set; }
    public int Code { get; set; }
}