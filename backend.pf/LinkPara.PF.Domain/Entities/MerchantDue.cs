using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class MerchantDue : AuditEntity, ITrackChange
{
    public Guid MerchantId { get; set; }
    public Merchant Merchant { get; set; }
    public Guid DueProfileId { get; set; }
    public DueProfile DueProfile { get; set; }
    public int TotalExecutionCount { get; set; }
    public DateTime LastExecutionDate { get; set; }
}