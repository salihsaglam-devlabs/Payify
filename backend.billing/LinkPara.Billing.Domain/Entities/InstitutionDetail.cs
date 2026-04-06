using LinkPara.Billing.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Billing.Domain.Entities;

public class InstitutionDetail : AuditEntity
{
    public Guid InstitutionSummaryId { get; set; }
    public DateTime ReconciliationDate { get; set; }
    public DateTime? BillDate { get; set; }
    public DateTime BillDueDate { get; set; }
    public string BillNumber { get; set; }
    public decimal BillAmount { get; set; }
    public string BillCurrency { get; set; }
    public decimal PaymentAmount { get; set; }
    public string PaymentCurrency { get; set; }
    public string PaymentReferenceId { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public ReconciliationStatus ReconciliationStatus { get; set; }
    public Guid VendorId { get; set; }
    public Guid InstitutionId { get; set; }
    public Guid? TransactionId { get; set; }
    public Transaction Transaction { get; set; }
}