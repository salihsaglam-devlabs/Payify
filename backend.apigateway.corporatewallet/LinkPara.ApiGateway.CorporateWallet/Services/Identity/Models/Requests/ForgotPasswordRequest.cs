namespace LinkPara.ApiGateway.CorporateWallet.Services.Identity.Models.Requests;

public class ForgotPasswordRequest
{
    public string Token { get; set; }
    public string NewPassword { get; set; }
    public string UserName { get; set; }
}
