namespace LinkPara.ApiGateway.Boa.Services.Identity.Models.Responses
{
    public class UserRefreshTokenResponse
    {
        public Guid UserId { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiration { get; set; }
    }
}
