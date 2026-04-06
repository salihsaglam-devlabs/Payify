
namespace LinkPara.HttpProviders.Receipt.Models;

public class ReceiptDto
{
    public Guid RefTransactionId { get; set; }
    public string Module { get; set; }
    public long ReceiptId { get; set; }
    public string ReceiptNumber { get; set; }
    public string TransactionType { get; set; }
    public string TransactionDirection { get; set; }
    public string PaymentMethod { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal Amount { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string CurrencyCode { get; set; }
    public string Description { get; set; }
    public string DetailInfo { get; set; }
}
