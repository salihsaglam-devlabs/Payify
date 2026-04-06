namespace LinkPara.ApiGateway.Services.Billing.Models.Responses;

public class InstitutionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid SectorId { get; set; }
    public string SectorName { get; set; }
    public Guid ActiveVendorId { get; set; }
    public string ActiveVendorName { get; set; }
}