using LinkPara.Card.Application.Commons.Constants;
using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.Locking;
using LinkPara.Card.Domain.Entities;
using LinkPara.Card.Domain.Enums;
using LinkPara.Card.Infrastructure.Persistence;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data;

namespace LinkPara.Card.Infrastructure.Services.Concurrency;

public class ProcessExecutionLockService : IProcessExecutionLockService
{
    private readonly CardDbContext _dbContext;
    private readonly ProcessExecutionLockSettings _settings;

    public ProcessExecutionLockService(
        CardDbContext dbContext,
        IOptions<ProcessExecutionLockSettings> settings)
    {
        _dbContext = dbContext;
        _settings = settings.Value;
    }

    public async Task<ProcessExecutionLockAcquireResult> TryAcquireAsync(
        string lockName,
        string source,
        string jobType,
        CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled)
        {
            return new ProcessExecutionLockAcquireResult
            {
                Acquired = true,
                Message = "Locking disabled."
            };
        }

        var pairedLockName = GetPairedLockName(lockName);
        var deadline = DateTime.Now.AddSeconds(Math.Max(1, _settings.Acquire.WaitTimeoutSeconds));
        string lastBlockingMessage = null;

        while (DateTime.Now <= deadline)
        {
            var now = DateTime.Now;
            await CleanupStaleLocksAsync(now, cancellationToken);

            var activeLock = await TryInsertActiveLockAsync(
                lockName,
                pairedLockName,
                source,
                jobType,
                now,
                cancellationToken);

            if (activeLock != null)
            {
                return new ProcessExecutionLockAcquireResult
                {
                    Acquired = true,
                    LockId = activeLock.Id,
                    Message = "Lock acquired."
                };
            }

            lastBlockingMessage = await BuildBlockingMessageAsync(lockName, pairedLockName, now, cancellationToken);

            if (DateTime.Now <= deadline)
            {
                await Task.Delay(Math.Max(1, _settings.Acquire.RetryIntervalMilliseconds), cancellationToken);
            }
        }

        return new ProcessExecutionLockAcquireResult
        {
            Acquired = false,
            Message = lastBlockingMessage ?? "Another process is running."
        };
    }

    public async Task ReleaseAsync(
        Guid? lockId,
        ProcessExecutionLockStatus finalStatus,
        string note = null,
        CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled || lockId is null)
        {
            return;
        }

        var now = DateTime.Now;
        await _dbContext.ProcessExecutionLocks
            .Where(x => x.Id == lockId.Value && x.Status == ProcessExecutionLockStatus.Active)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, finalStatus)
                .SetProperty(x => x.ReleasedAt, now)
                .SetProperty(x => x.Note, x => string.IsNullOrWhiteSpace(note) ? x.Note : note)
                .SetProperty(x => x.UpdateDate, now), cancellationToken);
    }

    public async Task ExtendAsync(
        Guid? lockId,
        CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled || lockId is null)
        {
            return;
        }

        var now = DateTime.Now;
        var lockState = await _dbContext.ProcessExecutionLocks
            .AsNoTracking()
            .Where(x => x.Id == lockId.Value
                        && x.Status == ProcessExecutionLockStatus.Active
                        && x.ReleasedAt == null)
            .Select(x => new
            {
                x.LockName,
                x.AcquiredAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (lockState == null)
        {
            return;
        }

        var policy = ResolvePolicy(lockState.LockName);
        var maxRunSeconds = Math.Max(1, policy.MaxRunSeconds);
        if (lockState.AcquiredAt.AddSeconds(maxRunSeconds) < now)
        {
            await _dbContext.ProcessExecutionLocks
                .Where(x => x.Id == lockId.Value
                            && x.Status == ProcessExecutionLockStatus.Active
                            && x.ReleasedAt == null)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.Status, ProcessExecutionLockStatus.TimedOut)
                    .SetProperty(x => x.ReleasedAt, now)
                    .SetProperty(x => x.Note, x => x.Note == null || x.Note == string.Empty
                        ? $"Timed out automatically after {maxRunSeconds} seconds."
                        : x.Note)
                    .SetProperty(x => x.UpdateDate, now), cancellationToken);
            return;
        }

        var nextExpiresAt = now.AddSeconds(GetLeaseTtlSeconds(lockState.LockName));
        await _dbContext.ProcessExecutionLocks
            .Where(x => x.Id == lockId.Value
                        && x.Status == ProcessExecutionLockStatus.Active
                        && x.ReleasedAt == null
                        && x.ExpiresAt >= now)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.ExpiresAt, nextExpiresAt)
                .SetProperty(x => x.UpdateDate, now), cancellationToken);
    }

    private static string GetPairedLockName(string lockName)
    {
        return lockName == ProcessLockNames.CardFileIngestion
            ? ProcessLockNames.CardReconciliation
            : ProcessLockNames.CardFileIngestion;
    }

    private static string BuildOwnerId()
    {
        return $"{Environment.MachineName}-{Guid.NewGuid():N}";
    }

    private async Task CleanupStaleLocksAsync(DateTime now, CancellationToken cancellationToken)
    {
        if (!_settings.Cleanup.Enabled)
        {
            return;
        }

        await _dbContext.ProcessExecutionLocks
            .Where(x => x.Status == ProcessExecutionLockStatus.Active && x.ReleasedAt != null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, ProcessExecutionLockStatus.Released)
                .SetProperty(x => x.Note, x => x.Note == null || x.Note == string.Empty
                    ? "Marked released automatically because ReleasedAt was already set."
                    : x.Note)
                .SetProperty(x => x.UpdateDate, now), cancellationToken);

        await _dbContext.ProcessExecutionLocks
            .Where(x => x.Status == ProcessExecutionLockStatus.Active && x.ExpiresAt < now)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, ProcessExecutionLockStatus.Expired)
                .SetProperty(x => x.ReleasedAt, now)
                .SetProperty(x => x.Note, x => x.Note == null || x.Note == string.Empty
                    ? "Expired automatically before new acquire."
                    : x.Note)
                .SetProperty(x => x.UpdateDate, now), cancellationToken);

        foreach (var lockName in GetKnownLockNames())
        {
            var policy = ResolvePolicy(lockName);
            var maxRunSeconds = Math.Max(1, policy.MaxRunSeconds);
            var maxRunThreshold = now.AddSeconds(-maxRunSeconds);
            await _dbContext.ProcessExecutionLocks
                .Where(x => x.Status == ProcessExecutionLockStatus.Active
                            && x.ReleasedAt == null
                            && x.LockName == lockName
                            && x.AcquiredAt < maxRunThreshold)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.Status, ProcessExecutionLockStatus.TimedOut)
                    .SetProperty(x => x.ReleasedAt, now)
                    .SetProperty(x => x.Note, x => x.Note == null || x.Note == string.Empty
                        ? $"Timed out automatically after {maxRunSeconds} seconds."
                        : x.Note)
                    .SetProperty(x => x.UpdateDate, now), cancellationToken);
        }
    }

    private async Task<ProcessExecutionLock> TryInsertActiveLockAsync(
        string lockName,
        string pairedLockName,
        string source,
        string jobType,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var executionStrategy = _dbContext.Database.CreateExecutionStrategy();
        return await executionStrategy.ExecuteAsync(async () =>
        {
            await using var tx = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            try
            {
                var hasBlockingLock = await HasBlockingLockAsync(lockName, pairedLockName, now, cancellationToken);
                if (hasBlockingLock)
                {
                    await tx.RollbackAsync(cancellationToken);
                    return null;
                }

                var activeLock = new ProcessExecutionLock
                {
                    Id = Guid.NewGuid(),
                    LockName = lockName,
                    OwnerId = BuildOwnerId(),
                    Status = ProcessExecutionLockStatus.Active,
                    Source = source,
                    JobType = jobType,
                    AcquiredAt = now,
                    ExpiresAt = now.AddSeconds(GetLeaseTtlSeconds(lockName)),
                    Note = "Lock acquired.",
                    CreateDate = now,
                    CreatedBy = AuditUsers.CardFileIngestion,
                    RecordStatus = RecordStatus.Active
                };

                _dbContext.ProcessExecutionLocks.Add(activeLock);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await tx.CommitAsync(cancellationToken);
                return activeLock;
            }
            catch (DbUpdateException ex)
            {
                await tx.RollbackAsync(cancellationToken);
                throw new InvalidOperationException(
                    $"Failed to persist process lock. lockName={lockName}, pairedLockName={pairedLockName}, jobType={jobType}, source={source}. Detail={ex.InnerException?.Message ?? ex.Message}",
                    ex);
            }
        });
    }

    private async Task<bool> HasBlockingLockAsync(
        string lockName,
        string pairedLockName,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var activeLocks = await GetActiveLockCandidatesAsync(lockName, pairedLockName, cancellationToken) ?? [];
        return activeLocks.Any(x => x.ExpiresAt >= now);
    }

    private async Task<string> BuildBlockingMessageAsync(
        string lockName,
        string pairedLockName,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var activeLocks = await GetActiveLockCandidatesAsync(lockName, pairedLockName, cancellationToken) ?? [];
        var activeBlocking = activeLocks
            .Where(x => x.ExpiresAt >= now)
            .OrderByDescending(x => x.AcquiredAt)
            .FirstOrDefault();

        if (activeBlocking == null)
        {
            return null;
        }

        return
            $"Blocking lock: {activeBlocking.LockName}, source: {activeBlocking.Source}, job: {activeBlocking.JobType}, acquiredAt: {activeBlocking.AcquiredAt:O}, expiresAt: {activeBlocking.ExpiresAt:O}";
    }

    private Task<List<ActiveLockSnapshot>> GetActiveLockCandidatesAsync(
        string lockName,
        string pairedLockName,
        CancellationToken cancellationToken)
    {
        return _dbContext.ProcessExecutionLocks
            .AsNoTracking()
            .Where(x => (x.LockName == lockName || x.LockName == pairedLockName)
                        && x.Status == ProcessExecutionLockStatus.Active
                        && x.ReleasedAt == null)
            .Select(x => new ActiveLockSnapshot
            {
                LockName = x.LockName,
                Source = x.Source,
                JobType = x.JobType,
                AcquiredAt = x.AcquiredAt,
                ExpiresAt = x.ExpiresAt
            })
            .ToListAsync(cancellationToken);
    }

    private class ActiveLockSnapshot
    {
        public string LockName { get; set; }
        public string Source { get; set; }
        public string JobType { get; set; }
        public DateTime AcquiredAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    private int GetLeaseTtlSeconds(string lockName)
    {
        if (_settings.Renewal.EnableHeartbeat != true)
        {
            var policy = ResolvePolicy(lockName);
            return Math.Max(1, policy.MaxRunSeconds);
        }

        var intervalSeconds = Math.Max(1, _settings.Renewal.IntervalSeconds);
        var missedHeartbeats = Math.Max(1, _settings.Renewal.MissedHeartbeatsBeforeExpire);
        var configuredTtl = Math.Max(1, _settings.Renewal.LeaseTtlSeconds);
        var minimumSafeTtl = intervalSeconds * (missedHeartbeats + 1);
        return Math.Max(configuredTtl, minimumSafeTtl);
    }

    private ProcessExecutionLockPolicySettings ResolvePolicy(string lockName)
    {
        if (_settings.Policies.TryGetValue(lockName, out var policy) && policy != null)
        {
            return policy;
        }

        var normalizedLockName = NormalizePolicyKey(lockName);
        var normalizedMatch = _settings.Policies
            .FirstOrDefault(x => NormalizePolicyKey(x.Key) == normalizedLockName)
            .Value;
        if (normalizedMatch != null)
        {
            return normalizedMatch;
        }

        return new ProcessExecutionLockPolicySettings();
    }

    private static IEnumerable<string> GetKnownLockNames()
    {
        yield return ProcessLockNames.CardFileIngestion;
        yield return ProcessLockNames.CardReconciliation;
    }

    private static string NormalizePolicyKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return new string(value
            .Where(char.IsLetterOrDigit)
            .Select(char.ToUpperInvariant)
            .ToArray());
    }
}
