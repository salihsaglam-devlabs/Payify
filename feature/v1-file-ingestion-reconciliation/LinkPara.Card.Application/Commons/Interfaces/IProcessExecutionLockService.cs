using LinkPara.Card.Domain.Enums;
using LinkPara.Card.Application.Commons.Models.Locking;

namespace LinkPara.Card.Application.Commons.Interfaces;

public interface IProcessExecutionLockService
{
    Task<ProcessExecutionLockAcquireResult> TryAcquireAsync(
        string lockName,
        string source,
        string jobType,
        CancellationToken cancellationToken = default);

    Task ReleaseAsync(
        Guid? lockId,
        ProcessExecutionLockStatus finalStatus,
        string note = null,
        CancellationToken cancellationToken = default);

    Task ExtendAsync(
        Guid? lockId,
        CancellationToken cancellationToken = default);
}
