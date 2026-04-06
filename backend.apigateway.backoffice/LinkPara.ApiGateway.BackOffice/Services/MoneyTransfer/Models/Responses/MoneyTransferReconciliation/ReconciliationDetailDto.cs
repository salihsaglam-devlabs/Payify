using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses.MoneyTransferReconciliation;

public class ReconciliationDetailDto
{
    public Guid Id { get; set; }
    public Guid ReconciliationSummaryId { get; set; }
    public string BankName { get; set; }
    public int BankCode { get; set; }
    public DateTime TransactionDate { get; set; }
    public BankTransactionType BankTransactionType { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; }
    public string BankReferenceNumber { get; set; }
    public DateTime Date { get; set; }
    public ReconciliationSummaryStatus SummaryStatus { get; set; }
    public ReconciliationDetailStatus DetailStatus { get; set; }
    public string NameSurname { get; set; }
    public string IbanNumber { get; set; }
    public Guid RelatedTableId { get; set; }
    public bool HasBankRecord { get; set; }
    public bool HasMoneyTransferRecord { get; set; }
}

