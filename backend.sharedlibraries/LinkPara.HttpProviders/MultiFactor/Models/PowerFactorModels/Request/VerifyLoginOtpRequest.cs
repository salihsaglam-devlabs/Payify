namespace LinkPara.HttpProviders.MultiFactor.Models.PowerFactorModels.Request;

public class VerifyLoginOtpRequest : BaseRequest
{
    public string LoginOtp { get; set; }
    public string CustomerId { get; set; }
    public List<string> ApplicationNames { get; set; }
}