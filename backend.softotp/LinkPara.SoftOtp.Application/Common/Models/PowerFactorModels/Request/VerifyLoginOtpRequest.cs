namespace LinkPara.SoftOtp.Application.Common.Models.PowerFactorModels.Request;

public class VerifyLoginOtpRequest : BaseRequest
{
    public string LoginOtp { get; set; }
    public long CustomerId { get; set; }
    public List<string> ApplicationNames { get; set; }
}