using System.Globalization;
using System.Text;
using LinkPara.Card.Application.Commons.Extensions;
using LinkPara.Card.Application.Commons.Helpers;
using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Configuration;
using LinkPara.Card.Domain.Entities.Reconciliation.Persistence;
using LinkPara.Card.Domain.Enums.Reconciliation;
using LinkPara.Card.Infrastructure.Persistence;
using LinkPara.Card.Infrastructure.Services.Audit;
using LinkPara.Card.Infrastructure.Services.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LinkPara.Card.Infrastructure.Services.Alert;

internal sealed class AlertService : IAlertService
{
    private const int PayloadPreviewLength = 1000;

    private readonly CardDbContext _dbContext;
    private readonly INotificationEmailService _notificationEmailService;
    private readonly IAuditStampService _auditStampService;
    private readonly ReconciliationOptions _options;
    private readonly ILogger<AlertService> _logger;
    private readonly IStringLocalizer _localizer;
    private readonly ITimeProvider _timeProvider;

    public AlertService(
        CardDbContext dbContext,
        INotificationEmailService notificationEmailService,
        IAuditStampService auditStampService,
        ITimeProvider timeProvider,
        IOptions<ReconciliationOptions> options,
        ILogger<AlertService> logger,
        Func<LinkPara.Card.Application.Commons.Localization.LocalizerResource, IStringLocalizer> localizerFactory)
    {
        _dbContext = dbContext;
        _notificationEmailService = notificationEmailService;
        _auditStampService = auditStampService;
        _timeProvider = timeProvider;
        _logger = logger;
        _options = options.Value;
        _localizer = localizerFactory(LinkPara.Card.Application.Commons.Localization.LocalizerResource.Messages);
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var alertOptions = _options.Alert;

        if (alertOptions.Enabled != true)
        {
            _logger.LogInformation(_localizer.Get("Alert.Disabled"));
            return;
        }

        var recipients = (alertOptions.ToEmails ?? Array.Empty<string>())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (recipients.Length == 0)
        {
            _logger.LogWarning(_localizer.Get("Alert.RecipientListEmpty"));
            return;
        }

        var toEmail = string.Join(";", recipients);
        var templateName = alertOptions.TemplateName;

        var validStatuses = alertOptions.IncludeFailed == true
            ? new[] { AlertStatus.Pending, AlertStatus.Failed }
            : new[] { AlertStatus.Pending };

        var alerts = await _dbContext.ReconciliationAlerts
            .AsNoTracking()
            .Where(x => validStatuses.Contains(x.AlertStatus))
            .OrderBy(x => x.CreateDate)
            .Take(alertOptions.BatchSize.Value)
            .Select(x => new ReconciliationAlert
            {
                Id = x.Id,
                FileLineId = x.FileLineId,
                GroupId = x.GroupId,
                EvaluationId = x.EvaluationId,
                OperationId = x.OperationId,
                Severity = x.Severity,
                AlertType = x.AlertType,
                Message = x.Message,
                AlertStatus = x.AlertStatus
            })
            .ToListAsync(cancellationToken);

        if (alerts.Count == 0)
        {
            _logger.LogInformation(_localizer.Get("Alert.NoAlertsToProcess"));
            return;
        }
        
        var evaluationIds = alerts.Select(a => a.EvaluationId).Distinct().ToArray();
        var operationIds = alerts.Where(a => a.OperationId != Guid.Empty).Select(a => a.OperationId).Distinct().ToArray();

        var evaluationsById = await _dbContext.ReconciliationEvaluations
            .AsNoTracking()
            .Where(e => evaluationIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, cancellationToken);

        var operationsById = operationIds.Length > 0
            ? await _dbContext.ReconciliationOperations
                .AsNoTracking()
                .Where(o => operationIds.Contains(o.Id))
                .ToDictionaryAsync(o => o.Id, cancellationToken)
            : new Dictionary<Guid, ReconciliationOperation>();

        var executionsByOpId = operationIds.Length > 0
            ? (await _dbContext.ReconciliationOperationExecutions
                .AsNoTracking()
                .Where(x => evaluationIds.Contains(x.EvaluationId) && operationIds.Contains(x.OperationId))
                .OrderBy(x => x.AttemptNumber)
                .ToListAsync(cancellationToken))
                .GroupBy(x => x.OperationId)
                .ToDictionary(g => g.Key, g => g.ToList())
            : new Dictionary<Guid, List<ReconciliationOperationExecution>>();

        foreach (var alert in alerts)
        {
            var processingUpdated = await TryMarkAsProcessingAsync(alert.Id, validStatuses, cancellationToken);
            if (!processingUpdated)
            {
                _logger.LogDebug(_localizer.Get("Alert.SkippedAlreadyPicked") + " AlertId={AlertId}", alert.Id);
                continue;
            }

            try
            {
                evaluationsById.TryGetValue(alert.EvaluationId, out var evaluation);
                ReconciliationOperation? operation = null;
                if (alert.OperationId != Guid.Empty)
                {
                    operationsById.TryGetValue(alert.OperationId, out operation);
                }

                List<ReconciliationOperationExecution> executions = [];
                if (alert.OperationId != Guid.Empty)
                {
                    executionsByOpId.TryGetValue(alert.OperationId, out executions);
                    executions ??= [];
                }

                var templateData = BuildTemplateData(alert, evaluation, operation, executions, _localizer, _timeProvider.Now);

                await _notificationEmailService.SendEmailAsync(
                    templateName,
                    templateData,
                    toEmail,
                    cancellationToken);

                await MarkAsConsumedAsync(alert.Id, cancellationToken);

                _logger.LogInformation(
                    _localizer.Get("Alert.ProcessedSuccessfully") + " AlertId={AlertId}, EvaluationId={EvaluationId}, OperationId={OperationId}",
                    alert.Id,
                    alert.EvaluationId,
                    alert.OperationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    _localizer.Get("Alert.ProcessingFailed") + " AlertId={AlertId}, EvaluationId={EvaluationId}, OperationId={OperationId}",
                    alert.Id,
                    alert.EvaluationId,
                    alert.OperationId);

                try
                {
                    await MarkAsFailedAsync(alert.Id, ExceptionDetailHelper.BuildDetailMessage(ex, 2000), cancellationToken);
                }
                catch (Exception markEx)
                {
                    _logger.LogError(
                        markEx,
                        _localizer.Get("Alert.ProcessingFailed") + " MarkAsFailedAsync also failed. AlertId={AlertId}",
                        alert.Id);
                }
            }
        }
    }

    private async Task<bool> TryMarkAsProcessingAsync(
        Guid alertId,
        IReadOnlyCollection<AlertStatus> allowedStatuses,
        CancellationToken cancellationToken)
    {
        var auditStamp = _auditStampService.CreateStamp();
        var rows = await _dbContext.ReconciliationAlerts
            .Where(x => x.Id == alertId && allowedStatuses.Contains(x.AlertStatus))
            .ExecuteUpdateAsync(update => update
                .SetProperty(x => x.AlertStatus, AlertStatus.Processing)
                .SetProperty(x => x.UpdateDate, auditStamp.Timestamp)
                .SetProperty(x => x.LastModifiedBy, auditStamp.UserId),
                cancellationToken);

        return rows > 0;
    }

    private async Task MarkAsConsumedAsync(Guid alertId, CancellationToken cancellationToken)
    {
        var auditStamp = _auditStampService.CreateStamp();
        await _dbContext.ReconciliationAlerts
            .Where(x => x.Id == alertId)
            .ExecuteUpdateAsync(update => update
                .SetProperty(x => x.AlertStatus, AlertStatus.Consumed)
                .SetProperty(x => x.UpdateDate, auditStamp.Timestamp)
                .SetProperty(x => x.LastModifiedBy, auditStamp.UserId),
                cancellationToken);
    }

    private async Task MarkAsFailedAsync(Guid alertId, string errorMessage, CancellationToken cancellationToken)
    {
        var currentMessage = await _dbContext.ReconciliationAlerts
            .AsNoTracking()
            .Where(x => x.Id == alertId)
            .Select(x => x.Message)
            .FirstOrDefaultAsync(cancellationToken);

        var mergedMessage = AppendError(currentMessage, errorMessage);

        var auditStamp = _auditStampService.CreateStamp();
        await _dbContext.ReconciliationAlerts
            .Where(x => x.Id == alertId)
            .ExecuteUpdateAsync(update => update
                .SetProperty(x => x.AlertStatus, AlertStatus.Failed)
                .SetProperty(x => x.Message, mergedMessage)
                .SetProperty(x => x.UpdateDate, auditStamp.Timestamp)
                .SetProperty(x => x.LastModifiedBy, auditStamp.UserId),
                cancellationToken);
    }

    private static Dictionary<string, string> BuildTemplateData(
        ReconciliationAlert alert,
        ReconciliationEvaluation? evaluation,
        ReconciliationOperation? operation,
        IReadOnlyCollection<ReconciliationOperationExecution> executions,
        IStringLocalizer localizer,
        DateTime now)
    {
        var culture = CultureInfo.InvariantCulture;
        var latestExecution = executions
            .OrderByDescending(x => x.AttemptNumber)
            .FirstOrDefault();

        var detailMessage = BuildDetailMessage(alert, evaluation, operation, executions, latestExecution, localizer, culture);

        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["emailsubject"] = localizer.Get("Alert.Template.Title"),
            ["templatetitle"] = localizer.Get("Alert.Template.Title"),
            ["templatedescription"] = localizer.Get("Alert.Template.Description"),
            ["labelalerttype"] = localizer.Get("Alert.Template.LabelAlertType"),
            ["labelseverity"] = localizer.Get("Alert.Template.LabelSeverity"),
            ["labelraisedat"] = localizer.Get("Alert.Template.LabelRaisedAt"),
            ["labelsummary"] = localizer.Get("Alert.Template.LabelSummary"),
            ["labelevaluation"] = localizer.Get("Alert.Template.LabelEvaluation"),
            ["labeloperation"] = localizer.Get("Alert.Template.LabelOperation"),
            ["labellatestexecution"] = localizer.Get("Alert.Template.LabelLatestExecution"),
            ["labelerror"] = localizer.Get("Alert.Template.LabelError"),
            ["labeltechnicaldetails"] = localizer.Get("Alert.Template.LabelTechnicalDetails"),
            
            ["alerttype"] = alert.AlertType ?? string.Empty,
            ["alertseverity"] = alert.Severity ?? string.Empty,
            ["raisedat"] = now.ToString("G", CultureInfo.InvariantCulture),
            ["summary"] = BuildSummary(alert, evaluation, operation, latestExecution, localizer),

            ["evaluationstatus"] = evaluation?.Status.ToString() ?? string.Empty,
            ["evaluationmessage"] = evaluation?.Message ?? string.Empty,

            ["operationcode"] = operation?.Code ?? string.Empty,
            ["operationstatus"] = operation?.Status.ToString() ?? string.Empty,
            ["operationnote"] = operation?.Note ?? string.Empty,

            ["lastexecutionstatus"] = latestExecution?.Status.ToString() ?? string.Empty,
            ["lastresultcode"] = latestExecution?.ResultCode ?? string.Empty,
            ["lastresultmessage"] = latestExecution?.ResultMessage ?? string.Empty,
            ["lasterrorcode"] = latestExecution?.ErrorCode ?? string.Empty,
            ["lasterrormessage"] = latestExecution?.ErrorMessage ?? string.Empty,
            ["error"] = latestExecution?.ErrorMessage ?? operation?.LastError ?? string.Empty,

            ["detailmessage"] = detailMessage
        };
    }

    private static string BuildSummary(
        ReconciliationAlert alert,
        ReconciliationEvaluation? evaluation,
        ReconciliationOperation? operation,
        ReconciliationOperationExecution? latestExecution,
        IStringLocalizer localizer)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(alert.AlertType))
            parts.Add($"{localizer.Get("Alert.Summary.AlertType")}: {alert.AlertType}");
        if (!string.IsNullOrWhiteSpace(alert.Severity))
            parts.Add($"{localizer.Get("Alert.Summary.Severity")}: {alert.Severity}");
        if (evaluation is not null)
            parts.Add($"{localizer.Get("Alert.Summary.EvaluationStatus")}: {evaluation.Status}");
        if (operation is not null && !string.IsNullOrWhiteSpace(operation.Code))
            parts.Add($"{localizer.Get("Alert.Summary.OperationCode")}: {operation.Code}");
        if (latestExecution is not null)
            parts.Add($"{localizer.Get("Alert.Summary.Execution")}: {latestExecution.Status} / {latestExecution.ResultCode}");
        if (!string.IsNullOrWhiteSpace(alert.Message))
            parts.Add($"{localizer.Get("Alert.Summary.Description")}: {alert.Message}");

        return string.Join(" | ", parts);
    }

    private static string BuildDetailMessage(
        ReconciliationAlert alert,
        ReconciliationEvaluation? evaluation,
        ReconciliationOperation? operation,
        IReadOnlyCollection<ReconciliationOperationExecution> executions,
        ReconciliationOperationExecution? latestExecution,
        IStringLocalizer localizer,
        CultureInfo culture)
    {
        var sb = new StringBuilder();

        AppendLine(sb, localizer.Get("Alert.Detail.AlertHeader"));
        AppendLine(sb, $"- {localizer.Get("Alert.Detail.AlertId")}: {alert.Id}");
        AppendLine(sb, $"- {localizer.Get("Alert.Detail.AlertType")}: {alert.AlertType ?? string.Empty}");
        AppendLine(sb, $"- {localizer.Get("Alert.Detail.Severity")}: {alert.Severity ?? string.Empty}");
        AppendLine(sb, $"- {localizer.Get("Alert.Detail.GroupId")}: {alert.GroupId}");
        AppendLine(sb, $"- {localizer.Get("Alert.Detail.FileLineId")}: {alert.FileLineId}");
        AppendLine(sb, $"- {localizer.Get("Alert.Detail.Message")}: {alert.Message ?? string.Empty}");
        AppendLine(sb);

        AppendLine(sb, localizer.Get("Alert.Detail.EvaluationHeader"));
        AppendLine(sb, $"- {localizer.Get("Alert.Detail.Id")}: {evaluation?.Id.ToString() ?? alert.EvaluationId.ToString()}");
        AppendLine(sb, $"- {localizer.Get("Alert.Detail.Status")}: {evaluation?.Status.ToString() ?? string.Empty}");
        AppendLine(sb, $"- {localizer.Get("Alert.Detail.Message")}: {evaluation?.Message ?? string.Empty}");
        AppendLine(sb, $"- {localizer.Get("Alert.Detail.CreatedOperationCount")}: {evaluation?.OperationCount.ToString() ?? "0"}");
        AppendLine(sb);

        AppendLine(sb, localizer.Get("Alert.Detail.OperationHeader"));
        AppendLine(sb, $"- {localizer.Get("Alert.Detail.Id")}: {GetOperationIdText(alert, operation)}");
        AppendLine(sb, $"- {localizer.Get("Alert.Detail.Code")}: {operation?.Code ?? string.Empty}");
        AppendLine(sb, $"- {localizer.Get("Alert.Detail.Status")}: {operation?.Status.ToString() ?? string.Empty}");
        AppendLine(sb, $"- {localizer.Get("Alert.Detail.Note")}: {operation?.Note ?? string.Empty}");
        AppendLine(sb, $"- {localizer.Get("Alert.Detail.Branch")}: {operation?.Branch ?? string.Empty}");
        AppendLine(sb, $"- {localizer.Get("Alert.Detail.LeaseOwner")}: {operation?.LeaseOwner ?? string.Empty}");
        AppendLine(sb, $"- {localizer.Get("Alert.Detail.RetryCount")}: {operation?.RetryCount.ToString() ?? string.Empty}");
        AppendLine(sb, $"- {localizer.Get("Alert.Detail.MaxRetries")}: {operation?.MaxRetries.ToString() ?? string.Empty}");
        AppendLine(sb, $"- {localizer.Get("Alert.Detail.NextAttemptAt")}: {operation?.NextAttemptAt?.ToString("G", culture) ?? string.Empty}");
        AppendLine(sb, $"- {localizer.Get("Alert.Detail.LastError")}: {operation?.LastError ?? string.Empty}");
        AppendLine(sb);

        AppendLine(sb, localizer.Get("Alert.Detail.LatestExecutionHeader"));
        AppendLine(sb, $"- {localizer.Get("Alert.Detail.AttemptNumber")}: {latestExecution?.AttemptNumber.ToString() ?? string.Empty}");
        AppendLine(sb, $"- {localizer.Get("Alert.Detail.Status")}: {latestExecution?.Status.ToString() ?? string.Empty}");
        AppendLine(sb, $"- {localizer.Get("Alert.Detail.StartedAt")}: {latestExecution?.StartedAt.ToString("G", culture) ?? string.Empty}");
        AppendLine(sb, $"- {localizer.Get("Alert.Detail.FinishedAt")}: {latestExecution?.FinishedAt?.ToString("G", culture) ?? string.Empty}");
        AppendLine(sb, $"- {localizer.Get("Alert.Detail.ResultCode")}: {latestExecution?.ResultCode ?? string.Empty}");
        AppendLine(sb, $"- {localizer.Get("Alert.Detail.ResultMessage")}: {latestExecution?.ResultMessage ?? string.Empty}");
        AppendLine(sb, $"- {localizer.Get("Alert.Detail.ErrorCode")}: {latestExecution?.ErrorCode ?? string.Empty}");
        AppendLine(sb, $"- {localizer.Get("Alert.Detail.ErrorMessage")}: {latestExecution?.ErrorMessage ?? string.Empty}");
        AppendLine(sb, $"- {localizer.Get("Alert.Detail.RequestPayloadPreview")}: {TrimPayload(latestExecution?.RequestPayload)}");
        AppendLine(sb, $"- {localizer.Get("Alert.Detail.ResponsePayloadPreview")}: {TrimPayload(latestExecution?.ResponsePayload)}");
        AppendLine(sb);

        AppendLine(sb, localizer.Get("Alert.Detail.ExecutionHistoryHeader"));
        if (executions.Count == 0)
        {
            AppendLine(sb, $"- {localizer.Get("Alert.NoExecutionRecord")}");
        }
        else
        {
            foreach (var execution in executions.OrderBy(x => x.AttemptNumber))
            {
                AppendLine(sb, $"- {localizer.Get("Alert.Detail.AttemptPrefix")} #{execution.AttemptNumber}");
                AppendLine(sb, $"  {localizer.Get("Alert.Detail.Status")}: {execution.Status}");
                AppendLine(sb, $"  {localizer.Get("Alert.Detail.StartedAt")}: {execution.StartedAt.ToString("G", culture)}");
                AppendLine(sb, $"  {localizer.Get("Alert.Detail.FinishedAt")}: {execution.FinishedAt?.ToString("G", culture) ?? "-"}");
                AppendLine(sb, $"  {localizer.Get("Alert.Detail.ResultCode")}: {execution.ResultCode ?? string.Empty}");
                AppendLine(sb, $"  {localizer.Get("Alert.Detail.ResultMessage")}: {execution.ResultMessage ?? string.Empty}");
                AppendLine(sb, $"  {localizer.Get("Alert.Detail.ErrorCode")}: {execution.ErrorCode ?? string.Empty}");
                AppendLine(sb, $"  {localizer.Get("Alert.Detail.ErrorMessage")}: {execution.ErrorMessage ?? string.Empty}");
                AppendLine(sb, $"  {localizer.Get("Alert.Detail.RequestPayloadPreview")}: {TrimPayload(execution.RequestPayload)}");
                AppendLine(sb, $"  {localizer.Get("Alert.Detail.ResponsePayloadPreview")}: {TrimPayload(execution.ResponsePayload)}");
            }
        }

        return sb.ToString();
    }

    private static string TrimPayload(string? payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
            return string.Empty;

        var normalized = payload.Replace(Environment.NewLine, " ").Replace("\n", " ").Replace("\r", " ").Trim();

        return normalized.Length <= PayloadPreviewLength
            ? normalized
            : normalized[..PayloadPreviewLength] + "...";
    }

    private static void AppendLine(StringBuilder sb, string? value = null)
    {
        if (value is null)
        {
            sb.AppendLine();
            return;
        }

        sb.AppendLine(value);
    }

    private static string GetOperationIdText(ReconciliationAlert alert, ReconciliationOperation? operation)
    {
        if (operation is not null)
            return operation.Id.ToString();

        return alert.OperationId == Guid.Empty ? string.Empty : alert.OperationId.ToString();
    }

    private static string AppendError(string? currentMessage, string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(currentMessage))
            return errorMessage;

        return $"{currentMessage} | {errorMessage}";
    }
}
