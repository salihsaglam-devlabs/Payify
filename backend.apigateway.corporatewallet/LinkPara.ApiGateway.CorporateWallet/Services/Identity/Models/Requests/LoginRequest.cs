namespace LinkPara.ApiGateway.CorporateWallet.Services.Identity.Models.Requests;

public class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string RecaptchaToken { get; set; }
}
