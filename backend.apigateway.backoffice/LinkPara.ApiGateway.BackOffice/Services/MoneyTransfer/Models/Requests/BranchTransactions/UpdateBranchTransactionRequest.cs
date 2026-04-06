using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests.BranchTransactions;

public class UpdateBranchTransactionRequest
{
    public Guid Id { get; set; }
    public BranchTransactionStatus BranchTransactionStatus { get; set; }
}