namespace LinkPara.ApiGateway.Services.Identity.Models.Requests;

public class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
    public bool RememberMe { get; set; }
    public string LoginOtp { get; set; }
    public string RecaptchaToken { get; set; }
}