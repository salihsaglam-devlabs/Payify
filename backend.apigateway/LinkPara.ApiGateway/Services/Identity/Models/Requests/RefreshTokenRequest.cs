namespace LinkPara.ApiGateway.Services.Identity.Models.Requests;

public class RefreshTokenRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string LastToken { get; set; }
    public string LoginOtp { get; set; }
}