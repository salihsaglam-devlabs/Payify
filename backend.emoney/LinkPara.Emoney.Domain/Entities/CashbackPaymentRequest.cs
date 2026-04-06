using LinkPara.HttpProviders.Cashback.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Emoney.Domain.Entities;

public class CashbackPaymentRequest : AuditEntity
{
    public Guid EntitlementId { get; set; }
    public CashbackPaymentStatus CashbackPaymentStatus { get; set; }
    public Guid TransactionId{ get; set; }
    public string WalletNumber { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; }


}
