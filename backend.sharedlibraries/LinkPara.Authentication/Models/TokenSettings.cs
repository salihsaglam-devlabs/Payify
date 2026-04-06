

namespace LinkPara.Authentication.Models
{
    public class TokenSettings
    {
        public TokenAuthenticationSettings TokenAuthenticationSettings { get; set; }
        public int TokenExpiryDefaultMinute { get; set; }
    }
}
