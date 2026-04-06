using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Responses;

public class VendorDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public RecordStatus RecordStatus { get; set; }
}