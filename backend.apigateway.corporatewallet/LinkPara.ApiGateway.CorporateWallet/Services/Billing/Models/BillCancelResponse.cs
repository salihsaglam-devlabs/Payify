namespace LinkPara.ApiGateway.CorporateWallet.Services.Billing.Models;

public class BillCancelResponse : BillingTransaction
{
    public BillCancelInvoice BillCancelInvoice { get; set; }
    public Guid? EmoneyTransactionId { get; set; }
}