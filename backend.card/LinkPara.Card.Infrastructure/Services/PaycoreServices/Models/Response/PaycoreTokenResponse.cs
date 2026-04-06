namespace LinkPara.Card.Infrastructure.Services.PaycoreServices.Models.Response;

public class PaycoreTokenResponse : PaycoreBaseResponse
{
    public int statusCode { get; set; }
    public string message { get; set; }
    public DateTime ExpireDate { get; set; }
    public TokenResult result { get; set; }
}
public class TokenResult
{
    public string token { get; set; }
}
