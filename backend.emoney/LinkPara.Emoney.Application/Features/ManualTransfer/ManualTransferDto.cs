using LinkPara.Emoney.Domain.Enums;

namespace LinkPara.Emoney.Application.Features.ManualTransfer;

public class ManualTransferDto
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