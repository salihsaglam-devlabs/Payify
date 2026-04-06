namespace LinkPara.HttpProviders.MultiFactor.Models.PowerFactorModels.Request;

public class VerifyLoginRequest
{
    public string LoginOtp { get; set; }
    public string PhoneNumber { get; set; }
}