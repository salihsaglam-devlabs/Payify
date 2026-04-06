namespace LinkPara.Billing.Application.Commons.Models.Billing;

public class BillCancelRequest : BillingTransaction
{
    public Bill Bill { get; set; }
    public string CancellationReason { get; set; }
}