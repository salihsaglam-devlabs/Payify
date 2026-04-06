namespace LinkPara.Billing.Infrastructure.ExternalServices.Billing.SekerBank.Models.Requests;

public class SekerBankBillInquiryRequest
{
    public string institutionOid { get; set; }
    public string subscriberNo1 { get; set; }
    public string subscriberNo2 { get; set; }
    public string subscriberNo3 { get; set; }
    public int termYear { get; set; }
    public int termMonth { get; set; }
}