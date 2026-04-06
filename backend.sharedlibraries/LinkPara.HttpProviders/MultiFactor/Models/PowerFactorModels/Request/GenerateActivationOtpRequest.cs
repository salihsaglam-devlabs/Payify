namespace LinkPara.HttpProviders.MultiFactor.Models.PowerFactorModels.Request;

public class GenerateActivationOtpRequest : BaseRequest
{
    public string CustomerId { get; set; }
    public string ApplicationName { get; set; }

}