using LinkPara.SharedModels.Persistence;

namespace LinkPara.Emoney.Domain.Entities;

public class ChangedBalanceLog : AuditEntity
{
    public string ConsentId { get; set; }
    public Guid AccountId { get; set; }
    public Guid WalletId { get; set; }
    public bool HasBalanceChanged { get; set; }
    public DateTime LastEventTime { get; set; }
    public virtual Account Account { get; set; }
    public virtual Wallet Wallet { get; set; }
}
