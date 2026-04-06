using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Application.Features.AcquireBanks;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Features.CostProfiles;

public class VposResponse : IMapFrom<Vpos>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public VposStatus VposStatus { get; set; }
    public SecurityType SecurityType { get; set; }
    public AcquireBankDto AcquireBank { get; set; }
}
