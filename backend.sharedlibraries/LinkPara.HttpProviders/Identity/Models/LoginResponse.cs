
namespace LinkPara.HttpProviders.Identity.Models
{
    public class LoginResponse
    {
        public Guid UserId { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiration { get; set; }
    }
}
