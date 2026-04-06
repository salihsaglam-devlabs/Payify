namespace LinkPara.SoftOtp.Application.Common.Models.PowerFactorModels.Request;

public class GenerateActivationOtpRequest : BaseRequest
{
    public long CustomerId { get; set; }
    public string ApplicationName { get; set; }

}