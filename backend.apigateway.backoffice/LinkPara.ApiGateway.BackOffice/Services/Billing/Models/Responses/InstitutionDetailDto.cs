using LinkPara.ApiGateway.BackOffice.Services.Billing.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Responses;

public class InstitutionDetailDto
{
    public Guid? TransactionId { get; set; }
    public string SubscriberName { get; set; }
    public string SubscriberNumber1 { get; set; }
    public string SubscriberNumber2 { get; set; }
    public string SubscriberNumber3 { get; set; }
    public string BillNumber { get; set; }
    public decimal BillAmount { get; set; }
    public string BillCurrency { get; set; }
    public string ReferenceId { get; set; }
    public DateTime? BillDate { get; set; }
    public DateTime? BillPaymentDate { get; set; }
    public DateTime? BillDueDate { get; set; }
    public DateTime ReconciliationDate { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public bool CanCancelTransaction { get; set; }
    public bool VendorExistingTransaction { get; set; }
    public bool ExistingTransaction { get; set; }
}