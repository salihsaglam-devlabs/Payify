namespace LinkPara.Billing.Application.Commons.Models.Billing;

public class BillPaymentResponse : BillingTransaction
{
    public BillInvoice Invoice { get; set; }
    public Guid? EmoneyTransactionId { get; set; }
}