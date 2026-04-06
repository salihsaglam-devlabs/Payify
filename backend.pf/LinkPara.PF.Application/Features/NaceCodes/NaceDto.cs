using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Application.Features.NaceCodes;

public class NaceDto : IMapFrom<Nace>
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