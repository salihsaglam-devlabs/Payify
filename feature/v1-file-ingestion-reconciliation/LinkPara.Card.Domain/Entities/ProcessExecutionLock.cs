using LinkPara.Card.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Card.Domain.Entities;

public class ProcessExecutionLock : AuditEntity
{
    public string LockName { get; set; }
    public string OwnerId { get; set; }
    public ProcessExecutionLockStatus Status { get; set; }
    public DateTime AcquiredAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? ReleasedAt { get; set; }
    public string Source { get; set; }
    public string JobType { get; set; }
    public string Note { get; set; }
}
