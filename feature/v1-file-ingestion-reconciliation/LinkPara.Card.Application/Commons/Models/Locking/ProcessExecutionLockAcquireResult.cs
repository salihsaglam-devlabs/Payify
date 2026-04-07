namespace LinkPara.Card.Application.Commons.Models.Locking;

public class ProcessExecutionLockAcquireResult
{
    public bool Acquired { get; set; }
    public Guid? LockId { get; set; }
    public string Message { get; set; } = string.Empty;
}
