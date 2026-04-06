namespace LinkPara.Billing.Infrastructure.ExternalServices.Billing.SekerBank.Models;

public class SekerBankBill
{
    public string oid { get; set; }
    public string billDate { get; set; }
    public string billNo { get; set; } 
    public string billDueDate { get; set; }
    public decimal billAmount { get; set; }
    public string billAmountMoneyType { get; set; }
    public string subscriberNo1 { get; set; }
    public string subscriberNo2 { get; set; }
    public string subscriberNo3 { get; set; }
}