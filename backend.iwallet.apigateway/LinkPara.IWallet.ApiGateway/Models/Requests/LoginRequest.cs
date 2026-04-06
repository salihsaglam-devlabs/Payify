namespace LinkPara.IWallet.ApiGateway.Models.Requests;

public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string Language { get; set; }
}
