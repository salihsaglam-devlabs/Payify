using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class BankLimit : AuditEntity, ITrackChange
{
    public Guid AcquireBankId { get; set; }
    public AcquireBank AcquireBank { get; set; }
    public decimal MonthlyLimitAmount { get; set; }
    public int MarginRatio { get; set; } 
    public decimal TotalAmount { get; set; }
    public BankLimitType BankLimitType { get; set; }
    public DateTime LastValidDate { get; set; }
    public bool IsExpired { get; set; }
}
