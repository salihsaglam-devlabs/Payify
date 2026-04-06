using LinkPara.Accounting.Domain.Enums;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting.Enums;
using LinkPara.SharedModels.Persistence;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkPara.Accounting.Domain.Entities;
public class Payment : AuditEntity
{
    public string ReferenceId { get; set; }
    public OperationType OperationType { get; set; }
    public bool HasCommission { get; set; }
    public string Source { get; set; }
    public string Destination { get; set; }
    public decimal Amount { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal BsmvAmount { get; set; }
    public decimal ReceiverCommissionAmount { get; set; }
    public decimal ReceiverBsmvAmount { get; set; }
    public DateTime TransactionDate { get; set; }
    public string CurrencyCode { get; set; }
    public bool IsSuccess { get; set; }
    public string ResultMessage { get; set; }
    public AccountingTransactionType AccountingTransactionType { get; set; }
    public int BankCode { get; set; }
    public Guid ClientReferenceId { get; set; }
    public bool IsCanceled { get; set; }
    public string CancelResultMessage { get; set; }
    public AccountingCustomerType AccountingCustomerType { get; set; }
    public string IbanNumber { get; set; }
    public int FailedPaymentRetryCount { get; set; } = 0;
    public Guid SenderInvoiceId { get; set; }
    public Guid ReceiverInvoiceId { get; set; }
    public PaymentInvoiceStatus SenderPaymentInvoiceStatus { get; set; }
    public PaymentInvoiceStatus ReceiverPaymentInvoiceStatus { get; set; }
    public string TransactionId { get; set; }
    public decimal ReturnAmount { get; set; }
    public decimal BankCommissionAmount { get; set; }
    public decimal ChargebackAmount { get; set; }
    public decimal SuspiciousAmount { get; set; }
    public decimal DueAmount { get; set; }
    public decimal ChargebackReturnAmount { get; set; }
    public decimal SuspiciousReturnAmount { get; set; }

    [NotMapped]
    public Guid? MerchantId { get; set; }
}
