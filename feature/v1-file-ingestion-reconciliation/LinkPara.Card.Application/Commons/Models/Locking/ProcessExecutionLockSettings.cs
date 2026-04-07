using LinkPara.Card.Application.Commons.Constants;

namespace LinkPara.Card.Application.Commons.Models.Locking;

public class ProcessExecutionLockSettings
{
    public bool Enabled { get; set; } = true;
    public ProcessExecutionLockAcquireSettings Acquire { get; set; } = new();
    public ProcessExecutionLockRenewalSettings Renewal { get; set; } = new();
    public ProcessExecutionLockCleanupSettings Cleanup { get; set; } = new();
    public Dictionary<string, ProcessExecutionLockPolicySettings> Policies { get; set; } =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [nameof(ProcessLockNames.CardFileIngestion)] = new ProcessExecutionLockPolicySettings { MaxRunSeconds = 1800 },
            [nameof(ProcessLockNames.CardReconciliation)] = new ProcessExecutionLockPolicySettings { MaxRunSeconds = 2700 }
        };
}

public class ProcessExecutionLockAcquireSettings
{
    public int WaitTimeoutSeconds { get; set; } = 5;
    public int RetryIntervalMilliseconds { get; set; } = 250;
}

public class ProcessExecutionLockRenewalSettings
{
    public bool EnableHeartbeat { get; set; } = true;
    public int IntervalSeconds { get; set; } = 20;
    public int LeaseTtlSeconds { get; set; } = 60;
    public int MissedHeartbeatsBeforeExpire { get; set; } = 2;
}

public class ProcessExecutionLockCleanupSettings
{
    public bool Enabled { get; set; } = true;
}

public class ProcessExecutionLockPolicySettings
{
    public int MaxRunSeconds { get; set; } = 1800;
}
