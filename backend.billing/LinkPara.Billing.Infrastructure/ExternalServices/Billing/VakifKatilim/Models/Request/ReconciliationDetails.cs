namespace LinkPara.Billing.Infrastructure.ExternalServices.Billing.VakifKatilim.Models.Request;

public class ReconciliationDetails
{
    public DateTime? ReconciliationDate { get; set; } 
    public bool? GroupByInstitution { get; set; } 
    public int? InstitutionId { get; set; }
}