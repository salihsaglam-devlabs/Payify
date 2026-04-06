namespace LinkPara.Billing.Application.Commons.Models.Billing;

public class BillInquiryResponse : BillingTransaction
{
    public List<Bill> Bills { get; set; }
}