using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

public class NaceDto
{
    public Guid Id { get; set; }
    public string SectorCode { get; set; }
    public string SectorDescription { get; set; }
    public string ProfessionCode { get; set; }
    public string ProfessionDescription { get; set; }
    public string Code { get; set; } 
    public string Description { get; set; }
    public RecordStatus RecordStatus { get; set; }
}