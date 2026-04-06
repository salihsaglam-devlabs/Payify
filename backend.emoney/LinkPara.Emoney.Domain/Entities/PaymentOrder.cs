using LinkPara.SharedModels.Persistence;

namespace LinkPara.Emoney.Domain.Entities;

public class PaymentOrder : AuditEntity
{
    public string ConsentNumber { get; set; }
    public DateTime ConsentCreateTime { get; set; }
    public string YosCode { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; }
    public string SenderTitle { get; set; }
    public string SenderWalletNumber { get; set; }
    public string ReceiverTitle { get; set; }
    public string ReceiverWalletNumber { get; set; }
    public string ReceiverIban { get; set; }
    public bool IsSuccess { get; set; }
    public Guid TransactionId { get; set; }
    public DateTime TransactionTime { get; set; }
}
