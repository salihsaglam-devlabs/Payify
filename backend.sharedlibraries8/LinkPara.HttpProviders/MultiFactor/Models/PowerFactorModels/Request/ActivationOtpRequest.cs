using Microsoft.Extensions.Localization;

namespace LinkPara.HttpProviders.MultiFactor.Models.PowerFactorModels.Request
{
    public class ActivationOtpRequest 
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
    }
}