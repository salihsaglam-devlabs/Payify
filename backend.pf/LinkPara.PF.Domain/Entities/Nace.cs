using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class Nace : AuditEntity
{
    public string SectorCode { get; set; }
    public string SectorDescription { get; set; }
    public string ProfessionCode { get; set; }
    public string ProfessionDescription { get; set; }
    public string Code { get; set; } 
    public string Description { get; set; }
}