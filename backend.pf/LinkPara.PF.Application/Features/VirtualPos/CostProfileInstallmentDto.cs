using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;

namespace LinkPara.PF.Application.Features.VirtualPos;

public class CostProfileInstallmentDto : IMapFrom<CostProfileInstallment>
{
    public Guid Id { get; set; }
    public int InstallmentSequence { get; set; }
    public int BlockedDayNumber { get; set; }
}