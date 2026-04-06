namespace LinkPara.Billing.Application.Commons.Models.Billing;

public class Bill
{
    public string Id { get; set; }
    public string Number { get; set; }
    public DateTime? Date { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public decimal CommissionAmount { get; set; }
    public string Currency { get; set; }
    public string SubscriberName { get; set; }
    public string SubscriberNumber1 { get; set; }
    public string SubscriberNumber2 { get; set; }
    public string SubscriberNumber3 { get; set; }
    public decimal BsmvAmount { get; set; }
}
