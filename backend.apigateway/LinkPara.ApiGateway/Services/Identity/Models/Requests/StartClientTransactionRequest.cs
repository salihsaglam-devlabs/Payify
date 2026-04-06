using LinkPara.ApiGateway.Services.Identity.Models.Enums;

namespace LinkPara.ApiGateway.Services.Identity.Models.Requests;

public class StartClientTransactionRequest
{
    public List<TransactionApprovementType> TransactionApprovementTypeList { get; set; }
    public bool CancelPendingTransactions { get; set; }
    public string TransactionContent { get; set; }
    public string TransactionName { get; set; }
    public int TransactionType { get; set; }
    public int TimeoutDuration { get; set; }
    public TransactionOwner TransactionOwner { get; set; }
}