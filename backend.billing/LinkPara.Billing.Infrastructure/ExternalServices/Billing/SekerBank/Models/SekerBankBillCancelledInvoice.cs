namespace LinkPara.Billing.Infrastructure.ExternalServices.Billing.SekerBank.Models;

public class SekerBankBillCancelledInvoice : SekerBankBill
{
    public string referenceId { get; set; }
    public string voucherNo { get; set; }
    public string cancelDescription { get; set; }
    public string cancelSuccess { get; set; }
    public string cancelDate { get; set; }
    public string cancelReferenceId { get; set; }
    public string cancelResult { get; set; }
}