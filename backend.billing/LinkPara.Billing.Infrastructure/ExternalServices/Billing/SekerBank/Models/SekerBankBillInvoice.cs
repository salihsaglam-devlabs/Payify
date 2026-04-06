namespace LinkPara.Billing.Infrastructure.ExternalServices.Billing.SekerBank.Models;

public class SekerBankBillInvoice : SekerBankBill
{
    public string referenceId { get; set; }
    public string voucherNo { get; set; }
    public string resultCode { get; set; }
    public string resultDesc { get; set; }
}