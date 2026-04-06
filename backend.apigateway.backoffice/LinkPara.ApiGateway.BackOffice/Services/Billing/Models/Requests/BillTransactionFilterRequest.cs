using LinkPara.ApiGateway.BackOffice.Services.Billing.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Requests;

public class BillTransactionFilterRequest : SearchQueryParams
{
    public Guid? VendorId { get; set; }
    public Guid? SectorId { get; set; }
    public Guid? InstitutionId { get; set; }
    public string BillNumber { get; set; }
    public string PayeeFullName { get; set; }
    public DateTime? BillDueStartDate { get; set; }
    public DateTime? BillDueEndDate { get; set; }
    public DateTime? PaymentStartDate { get; set; }
    public DateTime? PaymentEndDate { get; set; }
    public BillTransactionStatus? TransactionStatus { get; set; }
}