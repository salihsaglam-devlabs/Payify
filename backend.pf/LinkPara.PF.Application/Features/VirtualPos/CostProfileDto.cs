using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Features.VirtualPos;

public class CostProfileDto : IMapFrom<CostProfile>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateTime ActivationDate { get; set; }
    public decimal PointCommission { get; set; }
    public decimal ServiceCommission { get; set; }
    public ProfileStatus ProfileStatus { get; set; }
    public ProfileSettlementMode ProfileSettlementMode { get; set; }
    public List<CostProfileItemDto> CostProfileItems { get; set; }
}
