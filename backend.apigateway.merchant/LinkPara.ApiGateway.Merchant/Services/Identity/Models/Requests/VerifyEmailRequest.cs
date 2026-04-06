namespace LinkPara.ApiGateway.Merchant.Services.Identity.Models.Requests
{
    public class VerifyEmailRequest
    {
        public string Token { get; set; }
        public string Username { get; set; }
    }
}
