namespace LinkPara.ApiGateway.Merchant.Services.Identity.Models.Requests
{
    public class SendForgotPasswordEmailRequest
    {
        public string UserName { get; set; }
        public string RecaptchaToken { get; set; }
    }
}
