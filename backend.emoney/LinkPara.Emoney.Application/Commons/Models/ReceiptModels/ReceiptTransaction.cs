using LinkPara.Emoney.Domain.Enums;

namespace LinkPara.Emoney.Application.Commons.Models.ReceiptModels;

public sealed class ReceiptTransaction
{
    public Guid TransactionId { get; set; }
    public string TransactionDate { get; set; }

    public TransactionType TransactionType { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public TransactionDirection Direction { get; set; }

    public string CurrencyCode { get; set; }
    public string Display { get; set; }
    public string Description { get; set; }
    public string PaymentType { get; set; }
    public string Tag { get; set; }
    public Guid? ReturnedTransactionId { get; set; }
}
