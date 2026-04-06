using System.Text;
using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.Reconciliation;
using LinkPara.Card.Domain.Entities.Reconciliation;
using LinkPara.Card.Domain.Enums.Reconciliation;
using LinkPara.Card.Infrastructure.Persistence;
using LinkPara.Card.Infrastructure.Services.Audit;
using LinkPara.Card.Infrastructure.Services.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LinkPara.Card.Infrastructure.Services.AlertService;

internal sealed class AlertService : IAlertService
{
    private const string TurkishLanguage = "tr";
    private const string EnglishLanguage = "en";
    private const int PayloadPreviewLength = 1000;

    private readonly CardDbContext _dbContext;
    private readonly INotificationEmailService _notificationEmailService;
    private readonly IAuditStampService _auditStampService;
    private readonly ReconciliationOptions _options = new();
    private readonly ILogger<AlertService> _logger;

    public AlertService(
        CardDbContext dbContext,
        INotificationEmailService notificationEmailService,
        IAuditStampService auditStampService,
        IOptions<ReconciliationOptions> options,
        ILogger<AlertService> logger)
    {
        _dbContext = dbContext;
        _notificationEmailService = notificationEmailService;
        _auditStampService = auditStampService;
        _logger = logger;
        _options = options.Value;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var alertOptions = _options.Alert ?? new AlertOptions();

        if (!alertOptions.Enabled)
        {
            _logger.LogInformation("AlertService skipped because alert settings are disabled.");
            return;
        }

        var recipients = (alertOptions.ToEmails ?? Array.Empty<string>())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (recipients.Length == 0)
        {
            _logger.LogWarning("AlertService skipped because recipient list is empty.");
            return;
        }

        var toEmail = string.Join(";", recipients);
        var templateName = ResolveTemplateName(alertOptions);

        var validStatuses = alertOptions.IncludeFailed
            ? new[] { AlertStatus.Pending, AlertStatus.Failed }
            : new[] { AlertStatus.Pending };

        var alerts = await _dbContext.ReconciliationAlerts
            .AsNoTracking()
            .Where(x => validStatuses.Contains(x.AlertStatus))
            .OrderBy(x => x.CreateDate)
            .Take(alertOptions.BatchSize)
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
            _logger.LogInformation("AlertService found no alerts to process.");
            return;
        }

        foreach (var alert in alerts)
        {
            var processingUpdated = await TryMarkAsProcessingAsync(alert.Id, validStatuses, cancellationToken);
            if (!processingUpdated)
            {
                _logger.LogDebug("Alert skipped because it was already picked by another process. AlertId={AlertId}", alert.Id);
                continue;
            }

            try
            {
                var evaluation = await _dbContext.ReconciliationEvaluations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == alert.EvaluationId, cancellationToken);

                ReconciliationOperation? operation = null;
                if (alert.OperationId != Guid.Empty)
                {
                    operation = await _dbContext.ReconciliationOperations
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == alert.OperationId, cancellationToken);
                }

                var executions = new List<ReconciliationOperationExecution>();
                if (alert.OperationId != Guid.Empty)
                {
                    executions = await _dbContext.ReconciliationOperationExecutions
                        .AsNoTracking()
                        .Where(x => x.EvaluationId == alert.EvaluationId && x.OperationId == alert.OperationId)
                        .OrderBy(x => x.AttemptNumber)
                        .ToListAsync(cancellationToken);
                }

                var templateData = BuildTemplateData(alert, evaluation, operation, executions, alertOptions);

                await _notificationEmailService.SendEmailAsync(
                    templateName,
                    templateData,
                    toEmail,
                    cancellationToken);

                await MarkAsConsumedAsync(alert.Id, cancellationToken);

                _logger.LogInformation(
                    "Alert processed successfully. AlertId={AlertId}, EvaluationId={EvaluationId}, OperationId={OperationId}",
                    alert.Id,
                    alert.EvaluationId,
                    alert.OperationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "AlertService failed. AlertId={AlertId}, EvaluationId={EvaluationId}, OperationId={OperationId}",
                    alert.Id,
                    alert.EvaluationId,
                    alert.OperationId);

                await MarkAsFailedAsync(alert.Id, ex.Message, cancellationToken);
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
        AlertOptions alertOptions)
    {
        var language = ResolveLanguage(alertOptions);
        var latestExecution = executions
            .OrderByDescending(x => x.AttemptNumber)
            .FirstOrDefault();

        var detailMessage = BuildDetailMessage(alert, evaluation, operation, executions, latestExecution, language);

        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["alerttype"] = alert.AlertType ?? string.Empty,
            ["alertseverity"] = alert.Severity ?? string.Empty,
            ["raisedat"] = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"),
            ["summary"] = BuildSummary(alert, evaluation, operation, latestExecution, language),

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
        string language)
    {
        var parts = new List<string>();

        if (string.Equals(language, EnglishLanguage, StringComparison.OrdinalIgnoreCase))
        {
            if (!string.IsNullOrWhiteSpace(alert.AlertType))
                parts.Add($"Alert Type: {alert.AlertType}");
            if (!string.IsNullOrWhiteSpace(alert.Severity))
                parts.Add($"Severity: {alert.Severity}");
            if (evaluation is not null)
                parts.Add($"Evaluation Status: {evaluation.Status}");
            if (operation is not null && !string.IsNullOrWhiteSpace(operation.Code))
                parts.Add($"Operation Code: {operation.Code}");
            if (latestExecution is not null)
                parts.Add($"Execution: {latestExecution.Status} / {latestExecution.ResultCode}");
            if (!string.IsNullOrWhiteSpace(alert.Message))
                parts.Add($"Description: {alert.Message}");

            return string.Join(" | ", parts);
        }

        if (!string.IsNullOrWhiteSpace(alert.AlertType))
            parts.Add($"Uyari Tipi: {alert.AlertType}");
        if (!string.IsNullOrWhiteSpace(alert.Severity))
            parts.Add($"Oncelik: {alert.Severity}");
        if (evaluation is not null)
            parts.Add($"Evaluation Durumu: {evaluation.Status}");
        if (operation is not null && !string.IsNullOrWhiteSpace(operation.Code))
            parts.Add($"Operasyon Kodu: {operation.Code}");
        if (latestExecution is not null)
            parts.Add($"Execution: {latestExecution.Status} / {latestExecution.ResultCode}");
        if (!string.IsNullOrWhiteSpace(alert.Message))
            parts.Add($"Aciklama: {alert.Message}");

        return string.Join(" | ", parts);
    }

    private static string BuildDetailMessage(
        ReconciliationAlert alert,
        ReconciliationEvaluation? evaluation,
        ReconciliationOperation? operation,
        IReadOnlyCollection<ReconciliationOperationExecution> executions,
        ReconciliationOperationExecution? latestExecution,
        string language)
    {
        var sb = new StringBuilder();

        AppendLine(sb, "Alert");
        AppendLine(sb, $"- Alert Id: {alert.Id}");
        AppendLine(sb, $"- Alert Type: {alert.AlertType ?? string.Empty}");
        AppendLine(sb, $"- Severity: {alert.Severity ?? string.Empty}");
        AppendLine(sb, $"- Group Id: {alert.GroupId}");
        AppendLine(sb, $"- File Line Id: {alert.FileLineId}");
        AppendLine(sb, $"- Message: {alert.Message ?? string.Empty}");
        AppendLine(sb);

        AppendLine(sb, "Evaluation");
        AppendLine(sb, $"- Id: {evaluation?.Id.ToString() ?? alert.EvaluationId.ToString()}");
        AppendLine(sb, $"- Status: {evaluation?.Status.ToString() ?? string.Empty}");
        AppendLine(sb, $"- Message: {evaluation?.Message ?? string.Empty}");
        AppendLine(sb, $"- Created Operation Count: {evaluation?.CreatedOperationCount.ToString() ?? "0"}");
        AppendLine(sb);

        AppendLine(sb, "Operation");
        AppendLine(sb, $"- Id: {GetOperationIdText(alert, operation)}");
        AppendLine(sb, $"- Code: {operation?.Code ?? string.Empty}");
        AppendLine(sb, $"- Status: {operation?.Status.ToString() ?? string.Empty}");
        AppendLine(sb, $"- Note: {operation?.Note ?? string.Empty}");
        AppendLine(sb, $"- Branch: {operation?.Branch ?? string.Empty}");
        AppendLine(sb, $"- Lease Owner: {operation?.LeaseOwner ?? string.Empty}");
        AppendLine(sb, $"- Retry Count: {operation?.RetryCount.ToString() ?? string.Empty}");
        AppendLine(sb, $"- Max Retries: {operation?.MaxRetries.ToString() ?? string.Empty}");
        AppendLine(sb, $"- Next Attempt At: {operation?.NextAttemptAt?.ToString("dd.MM.yyyy HH:mm:ss") ?? string.Empty}");
        AppendLine(sb, $"- Last Error: {operation?.LastError ?? string.Empty}");
        AppendLine(sb);

        AppendLine(sb, "Latest Execution");
        AppendLine(sb, $"- Attempt Number: {latestExecution?.AttemptNumber.ToString() ?? string.Empty}");
        AppendLine(sb, $"- Status: {latestExecution?.Status.ToString() ?? string.Empty}");
        AppendLine(sb, $"- Started At: {latestExecution?.StartedAt.ToString("dd.MM.yyyy HH:mm:ss") ?? string.Empty}");
        AppendLine(sb, $"- Finished At: {latestExecution?.FinishedAt?.ToString("dd.MM.yyyy HH:mm:ss") ?? string.Empty}");
        AppendLine(sb, $"- Result Code: {latestExecution?.ResultCode ?? string.Empty}");
        AppendLine(sb, $"- Result Message: {latestExecution?.ResultMessage ?? string.Empty}");
        AppendLine(sb, $"- Error Code: {latestExecution?.ErrorCode ?? string.Empty}");
        AppendLine(sb, $"- Error Message: {latestExecution?.ErrorMessage ?? string.Empty}");
        AppendLine(sb, $"- Request Payload Preview: {TrimPayload(latestExecution?.RequestPayload)}");
        AppendLine(sb, $"- Response Payload Preview: {TrimPayload(latestExecution?.ResponsePayload)}");
        AppendLine(sb);

        AppendLine(sb, "Execution History");
        if (executions.Count == 0)
        {
            AppendLine(
                sb,
                string.Equals(language, EnglishLanguage, StringComparison.OrdinalIgnoreCase)
                    ? "- No execution record found."
                    : "- Execution kaydi bulunamadi.");
        }
        else
        {
            foreach (var execution in executions.OrderBy(x => x.AttemptNumber))
            {
                AppendLine(sb, $"- Attempt #{execution.AttemptNumber}");
                AppendLine(sb, $"  Status: {execution.Status}");
                AppendLine(sb, $"  Started: {execution.StartedAt:dd.MM.yyyy HH:mm:ss}");
                AppendLine(sb, $"  Finished: {execution.FinishedAt?.ToString("dd.MM.yyyy HH:mm:ss") ?? "-"}");
                AppendLine(sb, $"  Result Code: {execution.ResultCode ?? string.Empty}");
                AppendLine(sb, $"  Result Message: {execution.ResultMessage ?? string.Empty}");
                AppendLine(sb, $"  Error Code: {execution.ErrorCode ?? string.Empty}");
                AppendLine(sb, $"  Error Message: {execution.ErrorMessage ?? string.Empty}");
                AppendLine(sb, $"  Request Payload Preview: {TrimPayload(execution.RequestPayload)}");
                AppendLine(sb, $"  Response Payload Preview: {TrimPayload(execution.ResponsePayload)}");
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

    private static string ResolveTemplateName(AlertOptions alertOptions)
    {
        var language = ResolveLanguage(alertOptions);

        if (string.Equals(language, EnglishLanguage, StringComparison.OrdinalIgnoreCase))
        {
            return string.IsNullOrWhiteSpace(alertOptions.TemplateNameEn)
                ? "ReconciliationAlertTemplate_EN"
                : alertOptions.TemplateNameEn;
        }

        return string.IsNullOrWhiteSpace(alertOptions.TemplateNameTr)
            ? "ReconciliationAlertTemplate_TR"
            : alertOptions.TemplateNameTr;
    }

    private static string ResolveLanguage(AlertOptions alertOptions)
    {
        if (string.IsNullOrWhiteSpace(alertOptions.DefaultLanguage))
            return TurkishLanguage;

        return alertOptions.DefaultLanguage.Trim().ToLowerInvariant();
    }
}
