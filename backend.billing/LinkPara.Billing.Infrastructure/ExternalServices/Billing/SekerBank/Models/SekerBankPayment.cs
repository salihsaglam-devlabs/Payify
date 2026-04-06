namespace LinkPara.Billing.Infrastructure.ExternalServices.Billing.SekerBank.Models;

public class SekerBankPayment
{
    public string billDate { get; set; }
    public string billNo { get; set; }
    public string billDueDate { get; set; }
    public decimal billAmount { get; set; }
    public string billAmountMoneyType { get; set; }
    public decimal paymentAmount { get; set; }
    public string paymentAmountMoneyType { get; set; }
    public string paymentReferenceId { get; set; }
    public string subscriberNo1 { get; set; }
    public string subscriberNo2 { get; set; }
    public string subscriberNo3 { get; set; }
    public string subscriberNo4 { get; set; }
    public string subscriberNo5 { get; set; }
}