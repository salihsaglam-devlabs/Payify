namespace LinkPara.HttpProviders.MultiFactor.Models.PowerFactorModels.Response;

public class VerifyLoginResponse : PowerFactorResponseBase
{
    public bool Value { get; set; }
    public string CustomerId { get; set; }
}