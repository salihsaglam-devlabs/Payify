namespace LinkPara.IWallet.ApiGateway.Models.Responses;

public class LoginResponse
{
    public string Token { get; set; }
    public string TokenType { get; set; }
    public DateTime ExpireDate { get; set; }
    public string Name { get; set; }
}
