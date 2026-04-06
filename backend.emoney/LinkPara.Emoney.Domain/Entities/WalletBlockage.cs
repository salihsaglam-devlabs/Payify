using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Emoney.Domain.Entities;

public class WalletBlockage : AuditEntity
{
    public Guid WalletId { get; set; }
    public string WalletNumber { get; set; }
    public string AccountName { get; set; }
    public string WalletCurrencyCode { get; set; }
    public decimal CashBlockageAmount { get; set; }    
    public decimal CreditBlockageAmount { get; set; }
    public WalletBlockageOperationType OperationType { get; set; }
    public WalletBlockageStatus BlockageStatus { get; set; }
    public string BlockageType { get; set; }
    public string BlockageDescription { get; set; }
    public DateTime BlockageStartDate { get; set; }
    public DateTime? BlockageEndDate { get; set; }    
}

 