using LinkPara.HttpProviders.MultiFactor.Models.PowerFactorModels.Enums;
using LinkPara.HttpProviders.MultiFactor.Models.PowerFactorModels.Response;

namespace LinkPara.HttpProviders.MultiFactor.Models.PowerFactorModels.Request;

public class StartClientTransactionRequest
{
    public string ApplicationName { get; set; }
    public List<TransactionApprovementType> TransactionApprovementTypeList { get; set; }
    public bool CancelPendingTransactions { get; set; }
    public string TransactionContent { get; set; }
    public string TransactionName { get; set; }
    public int TransactionType { get; set; }
    public int TimeoutDuration { get; set; }
    public TransactionOwner TransactionOwner { get; set; }
}
