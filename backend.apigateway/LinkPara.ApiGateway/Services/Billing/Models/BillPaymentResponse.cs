namespace LinkPara.ApiGateway.Services.Billing.Models;

public class BillPaymentResponse : BillingTransaction
{
    public BillInvoice Invoice { get; set; }
    public Guid? EmoneyTransactionId { get; set; }
}