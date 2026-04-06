using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class CostProfileInstallment : AuditEntity, ITrackChange 
{
    public int InstallmentSequence { get; set; }
    public int BlockedDayNumber { get; set; }
    public Guid CostProfileItemId { get; set; }
    public CostProfileItem CostProfileItem { get; set; }
}