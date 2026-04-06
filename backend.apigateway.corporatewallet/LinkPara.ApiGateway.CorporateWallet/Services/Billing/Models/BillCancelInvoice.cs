namespace LinkPara.ApiGateway.CorporateWallet.Services.Billing.Models;

public class BillCancelInvoice : BillInvoice
{
    public DateTime CancelDate { get; set; }
    public string ResultDescription { get; set; }
}