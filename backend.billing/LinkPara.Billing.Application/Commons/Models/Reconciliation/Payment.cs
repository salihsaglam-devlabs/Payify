namespace LinkPara.Billing.Application.Commons.Models.Reconciliation;

public class Payment
{
    public DateTime BillDate { get; set; }
    public string BillNumber { get; set; }
    public DateTime BillDueDate { get; set; }
    public decimal BillAmount { get; set; }
    public string BillAmountCurrency { get; set; }
    public decimal PaymentAmount { get; set; }
    public string PaymentAmountCurrency { get; set; }
    public string PaymentReferenceId { get; set; }
    public string SubscriberNumber1 { get; set; }
    public string SubscriberNumber2 { get; set; }
    public string SubscriberNumber3 { get; set; }
}