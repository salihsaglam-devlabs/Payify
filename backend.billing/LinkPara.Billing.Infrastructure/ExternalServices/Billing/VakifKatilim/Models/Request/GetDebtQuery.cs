namespace LinkPara.Billing.Infrastructure.ExternalServices.Billing.VakifKatilim.Models.Request;

public class GetDebtQuery
{
    public int InstitutionId { get; set; }
    public string FirstQueryField { get; set; }
    public string SecondQueryField { get; set; }
}