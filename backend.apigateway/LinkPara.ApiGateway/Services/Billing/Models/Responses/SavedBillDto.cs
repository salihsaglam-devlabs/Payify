namespace LinkPara.ApiGateway.Services.Billing.Models.Responses;

public class SavedBillDto
{
    public Guid Id { get; set; }
    public Guid InstitutionId { get; set; }
    public string InstitutionName { get; set; }
    public string SectorName { get; set; }
    public string SubscriberNumber1 { get; set; }
    public string SubscriberNumber2 { get; set; }
    public string SubscriberNumber3 { get; set; }
    public string BillName { get; set; }
    public List<FieldDto> Fields { get; set; }
}