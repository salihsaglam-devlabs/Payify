using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class PricingProfileInstallment : AuditEntity, ITrackChange 
{
    public int InstallmentSequence { get; set; }
    public int BlockedDayNumber { get; set; }
    public Guid PricingProfileItemId { get; set; }
    public PricingProfileItem PricingProfileItem { get; set; }
}