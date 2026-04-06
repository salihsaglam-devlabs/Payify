namespace LinkPara.Billing.Application.Commons.Models.Billing;

public class BillCancelInvoice : BillInvoice
{
    public DateTime CancelDate { get; set; }
    public string ResultDescription { get; set; }
}