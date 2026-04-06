using LinkPara.Billing.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Billing.Domain.Entities;

public class Transaction : AuditEntity
{
    public Guid VendorId { get; set; }
    public Guid InstitutionId { get; set; }
    public Institution Institution { get; set; }
    public decimal BillAmount { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal BsmvAmount { get; set; }
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
    public Guid AccountingReferenceId { get; set; }
    public string ProvisionReferenceId { get; set; }
    public string SubscriberName { get; set; }
    public string PayeeFullName { get; set; }
    public string PayeeEmail { get; set; }
    public string PayeeMobile { get; set; }
    public string ServiceRequestId { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public TransactionStatus TransactionStatus { get; set; }
}