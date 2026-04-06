namespace LinkPara.Billing.Application.Commons.Models.Reconciliation;

public class ReconcilliationInstitutionDetailRequest
{
    public DateTime ReconciliationDate { get; set; }
    public Guid InstitutionId { get; set; }
    public Guid VendorId { get; set; }
}