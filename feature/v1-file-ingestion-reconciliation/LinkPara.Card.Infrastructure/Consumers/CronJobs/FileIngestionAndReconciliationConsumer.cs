using LinkPara.Card.Application.Commons.Constants;
using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.Locking;
using LinkPara.Card.Application.Commons.Models.Scheduler;
using LinkPara.Card.Domain.Enums;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LinkPara.Card.Infrastructure.Consumers.CronJobs;

public class FileIngestionAndReconciliationConsumer : IConsumer<FileIngestionAndReconciliationJob>
{
    private readonly IFileIngestionService _fileIngestionService;
    private readonly IReconciliationAutoOperationService _reconciliationAutoOperationService;
    private readonly IReconciliationService _reconciliationService;
    private readonly IProcessExecutionLockService _processExecutionLockService;
    private readonly ProcessExecutionLockSettings _lockSettings;
    private readonly ILogger<FileIngestionAndReconciliationConsumer> _logger;

    public FileIngestionAndReconciliationConsumer(
        IFileIngestionService fileIngestionService,
        IReconciliationAutoOperationService reconciliationAutoOperationService,
        IReconciliationService reconciliationService,
        IProcessExecutionLockService processExecutionLockService,
        IOptions<ProcessExecutionLockSettings> lockSettings,
        ILogger<FileIngestionAndReconciliationConsumer> logger)
    {
        _fileIngestionService = fileIngestionService;
        _reconciliationAutoOperationService = reconciliationAutoOperationService;
        _reconciliationService = reconciliationService;
        _processExecutionLockService = processExecutionLockService;
        _lockSettings = lockSettings.Value;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<FileIngestionAndReconciliationJob> context)
    {
        if (!Enum.IsDefined(typeof(FileIngestionAndReconciliationJobType), context.Message.ProcessType))
        {
            _logger.LogWarning(
                "Unknown scheduled process type received. ProcessType={ProcessType}, Trigger={TriggerSource}",
                context.Message.ProcessType,
                context.Message.TriggerSource);
            return;
        }

        var processType = (FileIngestionAndReconciliationJobType)context.Message.ProcessType;
        var triggerSource = string.IsNullOrWhiteSpace(context.Message.TriggerSource) ? "unknown" : context.Message.TriggerSource;
        var lockName = ResolveLockName(processType);
        var lockAcquire = await _processExecutionLockService.TryAcquireAsync(
            lockName: lockName,
            source: triggerSource,
            jobType: processType.ToString(),
            cancellationToken: context.CancellationToken);

        if (!lockAcquire.Acquired)
        {
            _logger.LogInformation(
                "Scheduled process skipped due to active lock. LockName={LockName}, ProcessType={ProcessType}, Trigger={TriggerSource}, Reason={Reason}",
                lockName,
                processType,
                triggerSource,
                lockAcquire.Message ?? "Process lock is busy.");
            return;
        }

        CancellationTokenSource heartbeatCancellationTokenSource = null;
        Task heartbeatTask = Task.CompletedTask;
        try
        {
            if (_lockSettings.Renewal.EnableHeartbeat)
            {
                heartbeatCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken);
                heartbeatTask = RunLockHeartbeatAsync(lockAcquire.LockId, heartbeatCancellationTokenSource.Token);
            }

            switch (processType)
            {
                case FileIngestionAndReconciliationJobType.ImportCardTransactionsFromFtp:
                {
                    var result = await _fileIngestionService.ImportCardTransactionsFromFtpAsync(cancellationToken: context.CancellationToken);
                    _logger.LogInformation(
                        "Card FTP ingestion completed. Trigger={TriggerSource}, FileCount={FileCount}",
                        triggerSource,
                        result.Count);
                    break;
                }
                case FileIngestionAndReconciliationJobType.ImportClearingFromFtp:
                {
                    var result = await _fileIngestionService.ImportClearingFromFtpAsync(cancellationToken: context.CancellationToken);
                    _logger.LogInformation(
                        "Clearing FTP ingestion completed. Trigger={TriggerSource}, FileCount={FileCount}",
                        triggerSource,
                        result.Count);
                    break;
                }
                case FileIngestionAndReconciliationJobType.ExecutePendingOperations:
                {
                    var take = context.Message.Take ?? 100;
                    var summary = await _reconciliationAutoOperationService.ExecutePendingOperationsAsync(
                        take,
                        cancellationToken: context.CancellationToken);

                    _logger.LogInformation(
                        "Reconciliation auto operations executed. Trigger={TriggerSource}, Requested={Requested}, Processed={Processed}, Succeeded={Succeeded}, Failed={Failed}",
                        triggerSource,
                        summary.RequestedCount,
                        summary.ProcessedCount,
                        summary.SucceededCount,
                        summary.FailedCount);
                    break;
                }
                case FileIngestionAndReconciliationJobType.RegenerateOperations:
                {
                    var take = context.Message.Take ?? 1000;
                    var summary = await _reconciliationService.RegenerateOperationsAsync(
                        actor: AuditUsers.CardFileIngestion,
                        take: take,
                        cancellationToken: context.CancellationToken);

                    _logger.LogInformation(
                        "Regenerate operations executed. Trigger={TriggerSource}, ReconciledCards={ReconciledCards}, ReconciliationOperations={ReconciliationOperations}, ReconciliationManualReviewItems={ReconciliationManualReviewItems}",
                        triggerSource,
                        summary.ReconciledCards,
                        summary.ReconciliationOperations,
                        summary.ReconciliationManualReviewItems);
                    break;
                }
                default:
                    _logger.LogWarning(
                        "Unhandled scheduled process type. ProcessType={ProcessType}, Trigger={TriggerSource}",
                        processType,
                        triggerSource);
                    break;
            }

            await _processExecutionLockService.ReleaseAsync(
                lockAcquire.LockId,
                ProcessExecutionLockStatus.Released,
                "Completed successfully.",
                CancellationToken.None);
        }
        catch (Exception ex)
        {
            await _processExecutionLockService.ReleaseAsync(
                lockAcquire.LockId,
                ProcessExecutionLockStatus.Failed,
                ex.Message,
                CancellationToken.None);
            throw;
        }
        finally
        {
            if (heartbeatCancellationTokenSource is not null)
            {
                try
                {
                    heartbeatCancellationTokenSource.Cancel();
                    await heartbeatTask;
                }
                catch (OperationCanceledException)
                {
                    // ignore
                }
                finally
                {
                    heartbeatCancellationTokenSource.Dispose();
                }
            }
        }
    }

    private static string ResolveLockName(FileIngestionAndReconciliationJobType processType)
    {
        return processType switch
        {
            FileIngestionAndReconciliationJobType.ImportCardTransactionsFromFtp => ProcessLockNames.CardFileIngestion,
            FileIngestionAndReconciliationJobType.ImportClearingFromFtp => ProcessLockNames.CardFileIngestion,
            FileIngestionAndReconciliationJobType.ExecutePendingOperations => ProcessLockNames.CardReconciliation,
            FileIngestionAndReconciliationJobType.RegenerateOperations => ProcessLockNames.CardReconciliation,
            _ => ProcessLockNames.CardReconciliation
        };
    }

    private async Task RunLockHeartbeatAsync(Guid? lockId, CancellationToken cancellationToken)
    {
        if (lockId is null)
        {
            return;
        }

        var heartbeatInterval = TimeSpan.FromSeconds(Math.Max(1, _lockSettings.Renewal.IntervalSeconds));
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(heartbeatInterval, cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                await _processExecutionLockService.ExtendAsync(lockId, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extend process lock heartbeat. LockId={LockId}", lockId);
            }
        }
    }
}
