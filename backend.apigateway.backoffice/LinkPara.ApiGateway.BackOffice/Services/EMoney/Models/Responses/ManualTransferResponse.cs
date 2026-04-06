using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;

public class ManualTransferResponse
{
    public Guid TransactionId { get; set; }
    public Dictionary<string, Guid> DocumentList { get; set; }
    public Guid ApprovalId { get; set; }
    public TransactionType TransactionType { get; set; }
    public string WalletNumber { get; set; }
    public string Name { get; set; }
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; }
}