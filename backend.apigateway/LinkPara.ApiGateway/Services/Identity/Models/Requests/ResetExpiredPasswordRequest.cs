namespace LinkPara.ApiGateway.Services.Identity.Models.Requests;

public class ResetExpiredPasswordRequest
{
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
    public string UserName { get; set; }
}