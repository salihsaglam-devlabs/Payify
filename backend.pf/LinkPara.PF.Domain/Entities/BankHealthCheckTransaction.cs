using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class BankHealthCheckTransaction : AuditEntity
{
    public TransactionType TransactionType { get; set; }
    public TransactionStatus TransactionStatus { get; set; }
    public int AcquireBankCode { get; set; }
    public DateTime BankTransactionDate { get; set; }
}
