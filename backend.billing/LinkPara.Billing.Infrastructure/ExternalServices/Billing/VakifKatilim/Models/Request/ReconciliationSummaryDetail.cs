namespace LinkPara.Billing.Infrastructure.ExternalServices.Billing.VakifKatilim.Models.Request;

public class ReconciliationSummaryDetail
{
    public DateTime? ReconciliationDate { get; set; } 
    public int? InstitutionId { get; set; }
    public bool? GetOnlyFaultyTransactions { get; set; } 
}