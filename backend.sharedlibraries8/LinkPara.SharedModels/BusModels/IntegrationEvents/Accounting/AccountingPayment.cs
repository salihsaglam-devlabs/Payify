using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting.Enums;

namespace LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting;

public class AccountingPayment
{
    public string ReferenceId { get; set; }
    public OperationType OperationType { get; set; }
    public bool HasCommission { get; set; }
    public string Source { get; set; }
    public string Destination { get; set; }
    public decimal Amount { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal ReceiverCommissionAmount { get; set; }
    public decimal BsmvAmount { get; set; }
    public decimal ReceiverBsmvAmount { get; set; }
    public DateTime TransactionDate { get; set; }
    public string CurrencyCode { get; set; }
    public int BankCode { get; set; }
    public AccountingCustomerType AccountingCustomerType { get; set; }
    public AccountingTransactionType AccountingTransactionType { get; set; }
    public Guid UserId { get; set; }
    public Guid ClientReferenceId { get; set; }
    public string IbanNumber { get; set; }
    public Guid TransactionId { get; set; }
    public Guid? MerchantId { get; set; }
    public decimal ReturnAmount { get; set; }
    public decimal BankCommissionAmount { get; set; }
    public decimal ChargebackAmount { get; set; }
    public decimal SuspiciousAmount { get; set; }
    public decimal DueAmount { get; set; }
    public decimal ChargebackReturnAmount { get; set; }
    public decimal SuspiciousReturnAmount { get; set; }
}