using System.Runtime.Serialization;

namespace LinkPara.Identity.Application.Common.Models.PowerFactorModels.Request
{
    public class VerifyLoginOtpRequest : BaseRequest
    {
        public string LoginOtp { get; set; }
        public string CustomerId { get; set; }
        public List<string> ApplicationNames { get; set; }
        public string PhoneNumber { get; set; }
    }
}