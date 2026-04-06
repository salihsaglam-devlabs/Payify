namespace LinkPara.ApiGateway.Card.Services.Identity.Models.Requests;

public class UserRefreshTokenRequest
{
    public Guid UserId { get; set; }
    public string RefreshToken { get; set; }
}
