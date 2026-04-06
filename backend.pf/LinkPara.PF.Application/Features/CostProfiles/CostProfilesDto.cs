using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Application.Features.VirtualPos;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Application.Features.CostProfiles;

public class CostProfilesDto : IMapFrom<CostProfile>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateTime ActivationDate { get; set; }
    public decimal PointCommission { get; set; }
    public decimal ServiceCommission { get; set; }
    public ProfileStatus ProfileStatus { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public DateTime CreateDate { get; set; }
    public List<CostProfileItemDto> CostProfileItems { get; set; }
    public PosType PosType { get; set; }
    public Guid? VposId { get; set; }
    public VposResponse Vpos { get; set; }
    public Guid? PhysicalPosId { get; set; }
    public PhysicalPosResponse PhysicalPos { get; set; }
    public ProfileSettlementMode ProfileSettlementMode { get; set; }
}
