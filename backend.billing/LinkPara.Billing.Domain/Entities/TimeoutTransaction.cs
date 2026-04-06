using LinkPara.Billing.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkPara.Billing.Domain.Entities;

public class TimeoutTransaction : AuditEntity
{
    public Guid VendorId { get; set; }
    public Guid InstitutionId { get; set; }
    public Institution Institution { get; set; }
    public decimal BillAmount { get; set; }
    public decimal CommissionAmount { get; set; }
    public string Currency { get; set; }
    public string BillId { get; set; }
    public string BillNumber { get; set; }
    public string SubscriptionNumber1 { get; set; }
    public string SubscriptionNumber2 { get; set; }
    public string SubscriptionNumber3 { get; set; }
    public string ReferenceId { get; set; }
    public string VoucherNumber { get; set; }
    public string Description { get; set; }
    public string WalletNumber { get; set; }
    public DateTime? BillDate { get; set; }
    public DateTime BillDueDate { get; set; }
    public DateTime PaymentDate { get; set; }
    public Guid UserId { get; set; }
    public Guid TransactionId { get; set; }
    public Guid AccountingReferenceId { get; set; }
    public string ProvisionReferenceId { get; set; }
    public string SubscriberName { get; set; }
    public string PayeeFullName { get; set; }
    public string PayeeEmail { get; set; }
    public string PayeeMobile { get; set; }
    public string ServiceRequestId { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public DateTime NextTryTime { get; set; }
    public TimeoutTransactionStatus TimeoutTransactionStatus { get; set; }
    public TimeoutTransactionType TimeoutTransactionType { get; set; }
}