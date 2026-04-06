using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities.PhysicalPos;

public class PhysicalPos : AuditEntity, ITrackChange
{
    public string Name { get; set; }
    public VposStatus VposStatus { get; set; }
    public Guid AcquireBankId { get; set; }
    public AcquireBank AcquireBank { get; set; }
    public VposType VposType { get; set; } // Unknown ata
    public string PfMainMerchantId { get; set; }
    public List<CostProfile> CostProfiles { get; set; }
    public List<MerchantPhysicalPos> MerchantPhysicalPosList { get; set; }
    public List<PhysicalPosCurrency> Currencies { get; set; }
}
