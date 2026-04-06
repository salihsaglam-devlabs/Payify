namespace LinkPara.Billing.Infrastructure.ExternalServices.Billing.VakifKatilim.Models.Response;

public class GetDebtQueryResponse
{
    public List<Bill> BillList { get; set; }
    public List<Result> Results { get; set; }
    public bool Success { get; set; }
}