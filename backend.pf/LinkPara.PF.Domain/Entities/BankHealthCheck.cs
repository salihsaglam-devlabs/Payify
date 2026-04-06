using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class BankHealthCheck : AuditEntity, ITrackChange
{
    public Guid AcquireBankId { get; set; }
    public AcquireBank AcquireBank { get; set; }
    public DateTime LastCheckDate { get; set; }
    public DateTime AllowedCheckDate { get; set; }
    public int TotalTransactionCount { get; set; }
    public int FailTransactionCount { get; set; }
    public int FailTransactionRate { get; set; }
    public HealthCheckType HealthCheckType { get; set; }
    public bool IsHealthCheckAllowed { get; set; }
}
