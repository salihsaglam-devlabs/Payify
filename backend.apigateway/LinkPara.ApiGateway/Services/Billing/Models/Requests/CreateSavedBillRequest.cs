namespace LinkPara.ApiGateway.Services.Billing.Models.Requests;

public class CreateSavedBillRequest
{
    public Guid InstitutionId { get; set; }
    public string SubscriberNumber1 { get; set; }
    public string SubscriberNumber2 { get; set; }
    public string SubscriberNumber3 { get; set; }
    public string BillName { get; set; }
}