using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Application.Features.AcquireBanks;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Application.Features.PhysicalPoses;

public class PhysicalPosDto : IMapFrom<Domain.Entities.PhysicalPos.PhysicalPos>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public VposStatus VposStatus { get; set; }
    public Guid AcquireBankId { get; set; }
    public AcquireBankDto AcquireBank { get; set; }
    public VposType VposType { get; set; } // Unknown ata
    public string PfMainMerchantId { get; set; }
    public bool? HasActiveCostProfile { get; set; }
    public DateTime CreateDate { get; set; }
    public RecordStatus RecordStatus { get; set; }
}
