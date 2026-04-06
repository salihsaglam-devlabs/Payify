namespace LinkPara.ApiGateway.BackOffice.Services.Billing.Models;

public class Bill
{
    public string Id { get; set; }
    public string Number { get; set; }
    public DateTime Date { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public decimal CommissionAmount { get; set; }
    public string Currency { get; set; }
    public string SubscriberName { get; set; }
    public string SubscriberNumber1 { get; set; }
    public string SubscriberNumber2 { get; set; }
    public string SubscriberNumber3 { get; set; }
}