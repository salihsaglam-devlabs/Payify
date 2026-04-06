namespace LinkPara.HttpProviders.MultiFactor.Models.PowerFactorModels.Response;

public class StartOneTouchTransactionResponse : PowerFactorResponseBase
{
    public string TransactionToken { get; set; }
    public string PushToken { get; set; }
}