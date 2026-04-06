namespace LinkPara.Identity.Application.Common.Models.PowerFactorModels.Request
{
    public class GenerateActivationOtpRequest : BaseRequest
    {
        public string CustomerId { get; set; }
        public string ApplicationName { get; set; }

    }
}