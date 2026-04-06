namespace LinkPara.ApiGateway.Services.Identity.Models.Requests
{
    public class UserRefreshTokenRequest
    {
        public Guid UserId { get; set; }
        public string RefreshToken { get; set; }

        public DateTime RefreshTokenExpiration { get; set; }
    }
}
