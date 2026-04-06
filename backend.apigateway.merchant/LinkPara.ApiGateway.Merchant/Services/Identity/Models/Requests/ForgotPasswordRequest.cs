namespace LinkPara.ApiGateway.Merchant.Services.Identity.Models.Requests;

public class ForgotPasswordRequest
{
    public string Token { get; set; }
    public string NewPassword { get; set; }
    public string UserName { get; set; }
}
