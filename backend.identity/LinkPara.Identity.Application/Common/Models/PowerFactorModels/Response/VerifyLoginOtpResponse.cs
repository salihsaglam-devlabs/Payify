namespace LinkPara.Identity.Application.Common.Models.PowerFactorModels.Response
{
    public class VerifyLoginOtpResponse : PowerFactorResponseBase
    {
        public bool Value { get; set; }
        public string CustomerId { get; set; }
    }
}