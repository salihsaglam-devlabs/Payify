namespace LinkPara.Billing.Application.Commons.Models.Billing;

public class BillCancelResponse : BillingTransaction
{
    public BillCancelInvoice BillCancelInvoice { get; set; }
    public Guid? EmoneyTransactionId { get; set; }
}