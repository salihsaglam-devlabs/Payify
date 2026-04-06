using System.ComponentModel.DataAnnotations.Schema;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.DomainEvents;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Emoney.Domain.Entities;

public class Wallet : AuditEntity, IHasDomainEvent
{
    public string WalletNumber { get; set; }
    public WalletType WalletType { get; set; }
    public string FriendlyName { get; set; }
    public string CurrencyCode { get; set; }
    public Currency Currency { get; set; }    
    public DateTime LastActivityDate { get; set; }
    public List<Transaction> Transactions { get; set; }
    public bool IsMainWallet { get; set; }
    public List<DomainEvent> DomainEvents { get; set; } = new List<DomainEvent>();
    public bool IsBlocked { get; set; }

    public bool ExceededLimitsThisMonth { get; set; }

    public Guid AccountId { get; set; }
    public Account Account { get; set; }

    public DateTime? OpeningDate { get; set; }
    public DateTime? ClosingDate { get; set; }

    public decimal CurrentBalanceCredit { get; set; }
    public decimal CurrentBalanceCash { get; set; }   
    public decimal BlockedBalance { get; set; }    
    public decimal BlockedBalanceCredit { get; set; }
    public decimal AvailableBalanceCredit => CurrentBalanceCredit - BlockedBalanceCredit;
    public decimal AvailableBalanceCash => CurrentBalanceCash - BlockedBalance;
    public decimal AvailableBalance => AvailableBalanceCash + AvailableBalanceCredit;
}