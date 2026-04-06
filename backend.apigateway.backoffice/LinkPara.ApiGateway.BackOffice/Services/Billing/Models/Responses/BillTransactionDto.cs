using LinkPara.ApiGateway.BackOffice.Services.Billing.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Responses;

public class BillTransactionDto
{
    public Guid Id { get; set; }
    public Guid VendorId { get; set; }
    public string VendorName { get; set; }
    public Guid SectorId { get; set; }
    public string SectorName { get; set; }
    public Guid InstitutionId { get; set; }
    public string InstitutionName { get; set; }
    public decimal BillAmount { get; set; }
    public string Currency { get; set; }
    public string BillNumber { get; set; }
    public string SubscriptionNumber1 { get; set; }
    public string SubscriptionNumber2 { get; set; }
    public string SubscriptionNumber3 { get; set; }
    public DateTime? BillDate { get; set; }
    public DateTime BillDueDate { get; set; }
    public DateTime PaymentDate { get; set; }
    public string UserId { get; set; }
    public string SubscriberName { get; set; }
    public string PayeeFullName { get; set; }
    public string PayeeEmail { get; set; }
    public string PayeeMobile { get; set; }
    public string ServiceRequestId { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public BillTransactionStatus TransactionStatus { get; set; }
}