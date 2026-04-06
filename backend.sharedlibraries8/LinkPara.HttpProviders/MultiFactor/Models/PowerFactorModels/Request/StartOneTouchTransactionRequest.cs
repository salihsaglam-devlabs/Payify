namespace LinkPara.HttpProviders.MultiFactor.Models.PowerFactorModels.Request;

public class StartOneTouchTransactionRequest
{
    public string TransactionDefinitionKey { get; set; }
    public int[] TransactionApprovementTypeList { get; set; }
    public string TransactionContent { get; set; }
    public string TransactionName { get; set; }
    public int TransactionType { get; set; }
    public int TimeoutDuration { get; set; }
    public string UserName { get; set; }
}
