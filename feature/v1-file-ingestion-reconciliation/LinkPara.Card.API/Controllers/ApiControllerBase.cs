using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.Locking;
using LinkPara.Card.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LinkPara.Card.API.Controllers;

[ApiController]
[Route("v1/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected ISender Mediator => 
        HttpContext.RequestServices.GetRequiredService<ISender>();

    protected IProcessExecutionLockService ProcessExecutionLockService =>
        HttpContext.RequestServices.GetRequiredService<IProcessExecutionLockService>();

    protected ProcessExecutionLockSettings ProcessExecutionLockSettings =>
        HttpContext.RequestServices.GetRequiredService<IOptions<ProcessExecutionLockSettings>>().Value;

    protected ILogger<ApiControllerBase> Logger =>
        HttpContext.RequestServices.GetRequiredService<ILogger<ApiControllerBase>>();

    protected async Task<ActionResult<T>> ExecuteWithProcessLockAsync<T>(
        string lockName,
        string jobType,
        Func<CancellationToken, Task<T>> action,
        CancellationToken cancellationToken)
    {
        var acquireResult = await ProcessExecutionLockService.TryAcquireAsync(
            lockName: lockName,
            source: "API",
            jobType: jobType,
            cancellationToken: cancellationToken);

        if (!acquireResult.Acquired)
        {
            return StatusCode(
                StatusCodes.Status423Locked,
                acquireResult.Message ?? $"Process lock is busy for {lockName}");
        }

        CancellationTokenSource heartbeatCancellationTokenSource = null;
        Task heartbeatTask = Task.CompletedTask;
        try
        {
            if (ProcessExecutionLockSettings.Renewal.EnableHeartbeat)
            {
                heartbeatCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                heartbeatTask = RunLockHeartbeatAsync(
                    acquireResult.LockId,
                    heartbeatCancellationTokenSource.Token);
            }

            var result = await action(cancellationToken);
            await ProcessExecutionLockService.ReleaseAsync(
                acquireResult.LockId,
                ProcessExecutionLockStatus.Released,
                "Completed successfully.",
                CancellationToken.None);
            return result;
        }
        catch (Exception ex)
        {
            await ProcessExecutionLockService.ReleaseAsync(
                acquireResult.LockId,
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

    private async Task RunLockHeartbeatAsync(
        Guid? lockId,
        CancellationToken cancellationToken)
    {
        if (lockId is null)
        {
            return;
        }

        var heartbeatInterval = TimeSpan.FromSeconds(Math.Max(1, ProcessExecutionLockSettings.Renewal.IntervalSeconds));
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(heartbeatInterval, cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                await ProcessExecutionLockService.ExtendAsync(lockId, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to extend process lock heartbeat in API pipeline. LockId={LockId}", lockId);
            }
        }
    }
}
