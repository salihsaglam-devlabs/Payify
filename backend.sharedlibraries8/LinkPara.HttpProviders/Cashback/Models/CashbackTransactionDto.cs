
namespace LinkPara.HttpProviders.Cashback.Models;

public class CashbackTransactionDto 
{
    public Guid OriginalTransactionId { get; set; }
    public string TransactionType { get; set; }
    public string PaymentMethod { get; set; }
    public string TransactionStatus { get; set; }
    public string TransactionDirection { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; }
    public DateTime TransactionDate { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; }
    public Guid WalletId { get; set; }
    public string WalletNo { get; set; }
    public string CorporateWalletNumber { get; set; }
    public string CorporateAccountName { get; set; }
    public string AccountKycLevel { get; set; }
    public string MccCode { get; set; }
    public string ConversationId { get; set; }
}
