using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Emoney.Domain.Entities;

public class AccountActivity : AuditEntity
{
    public Guid AccountId { get; set; }
    public string TransferType { get; set; }
    public string Sender { get; set; }
    public TransactionDirection TransactionDirection { get; set; }
    public string Receiver { get; set; }
    public decimal Amount { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public bool OwnAccount { get; set; }
}