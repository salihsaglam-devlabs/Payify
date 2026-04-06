using LinkPara.HttpProviders.MoneyTransfer.Models.Enums;

namespace LinkPara.HttpProviders.MoneyTransfer.Models;
public class PfReturnTransactionDto
{
    public Guid Id { get; set; }
    public string SenderName { get; set; }
    public string OrderNumber { get; set; }
    public string MerchantNumber { get; set; }
    public string BankReferenceNumber { get; set; }
    public decimal Amount { get; set; }
    public string SenderIban { get; set; }
    public string SenderIdentityNumber { get; set; }
    public DateTime TransactionDate { get; set; }
    public string Description { get; set; }
    public PfReturnTransactionStatus TransactionStatus { get; set; }
    public string ErrorMessage { get; set; }
    public string ReturnTransactionId { get; set; }
    public string ReturnBankReferenceNumber { get; set; }
    public DateTime ReturnTransactionDate { get; set; }
}
