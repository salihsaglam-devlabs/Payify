using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.FileIngestion;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Orchestration;

public class ReconciliationAlarmService : IReconciliationAlarmService
{
    private readonly INotificationEmailService _notificationEmailService;
    private readonly FileIngestionSettings _settings;
    private readonly ILogger<ReconciliationAlarmService> _logger;

    public ReconciliationAlarmService(
        INotificationEmailService notificationEmailService,
        IOptions<FileIngestionSettings> options,
        ILogger<ReconciliationAlarmService> logger)
    {
        _notificationEmailService = notificationEmailService;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task RaiseAsync(
        string alarmCode,
        string summary,
        IReadOnlyDictionary<string, string> metadata,
        CancellationToken cancellationToken = default)
    {
        var alarmSettings = _settings.Alarm ?? new ReconciliationAlarmSettings();
        if (!alarmSettings.Enabled)
        {
            return;
        }

        var recipients = (alarmSettings.ToEmails ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (recipients.Length == 0)
        {
            _logger.LogWarning("Reconciliation alarm skipped due to empty recipients. AlarmCode={AlarmCode}", alarmCode);
            return;
        }

        var payload = BuildTemplateData(alarmCode, summary, metadata);
        var templateName = alarmSettings.TemplateName;

        foreach (var recipient in recipients)
        {
            await SendInformationMailAsync(payload, recipient, templateName, cancellationToken);
        }
    }

    private static Dictionary<string, string> BuildTemplateData(
        string alarmCode,
        string summary,
        IReadOnlyDictionary<string, string> metadata)
    {
        var payload = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["alarmCode"] = alarmCode ?? string.Empty,
            ["summary"] = summary ?? string.Empty,
            ["raisedAt"] = DateTime.Now.ToString("O"),
            ["cardTransactionRecordId"] = string.Empty,
            ["cardTransactionId"] = string.Empty,
            ["oceanTxnGuid"] = string.Empty,
            ["runId"] = string.Empty,
            ["reconciliationOperationId"] = string.Empty,
            ["manualReviewItemId"] = string.Empty,
            ["operationCode"] = string.Empty,
            ["operationReasonCode"] = string.Empty,
            ["operationReasonText"] = string.Empty,
            ["duplicateGroupKey"] = string.Empty,
            ["duplicateCount"] = string.Empty,
            ["sampleKeys"] = string.Empty,
            ["fileName"] = string.Empty,
            ["fileType"] = string.Empty,
            ["sourceType"] = string.Empty,
            ["autoOperationCount"] = string.Empty,
            ["error"] = string.Empty,
            ["note"] = string.Empty
        };

        if (metadata is null)
        {
            return payload;
        }

        foreach (var (key, value) in metadata)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                payload[key] = value ?? string.Empty;
            }
        }

        if (string.IsNullOrWhiteSpace(payload["cardTransactionRecordId"]) &&
            !string.IsNullOrWhiteSpace(payload["cardTransactionId"]))
        {
            payload["cardTransactionRecordId"] = payload["cardTransactionId"];
        }

        if (string.IsNullOrWhiteSpace(payload["cardTransactionId"]) &&
            !string.IsNullOrWhiteSpace(payload["cardTransactionRecordId"]))
        {
            payload["cardTransactionId"] = payload["cardTransactionRecordId"];
        }

        return payload;
    }

    private async Task SendInformationMailAsync(
        IReadOnlyDictionary<string, string> templateData,
        string email,
        string templateName,
        CancellationToken cancellationToken)
    {
        await _notificationEmailService.SendEmailAsync(templateName, templateData, email, cancellationToken);
    }
}
