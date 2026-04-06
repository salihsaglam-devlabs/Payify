namespace LinkPara.HttpProviders.MultiFactor.Models.PowerFactorModels.Request;

public class UpdateActivationPINByCustomerIdRequest
{
    public long CustomerId { get; set; }
    public string PIN { get; set; }
}
