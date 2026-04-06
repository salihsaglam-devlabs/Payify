namespace LinkPara.Identity.Application.Common.Models.AccountModels;

public class RegisterResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime RefreshTokenExpiration { get; set; }
    public Guid UserId { get; set; }
}