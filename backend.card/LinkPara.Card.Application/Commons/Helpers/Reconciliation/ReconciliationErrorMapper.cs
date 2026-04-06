using System.Text;
using LinkPara.Card.Application.Commons.Models.Reconciliation;

namespace LinkPara.Card.Application.Commons.Helpers.Reconciliation;

public static class ReconciliationErrorMapper
{
    public static ReconciliationErrorDetail MapException(
        Exception ex,
        string step,
        Guid? fileLineId = null,
        Guid? operationId = null,
        Guid? evaluationId = null,
        string? detail = null,
        string? message = null)
    {
        ArgumentNullException.ThrowIfNull(ex);

        var (code, defaultMessage, defaultDetail) = ClassifyException(ex);

        var finalMessage = BuildUserFacingMessage(message ?? defaultMessage, ex);
        var finalDetail = detail ?? BuildDetailMessage(ex);

        return new ReconciliationErrorDetail
        {
            Code = code,
            Message = finalMessage,
            Detail = string.IsNullOrWhiteSpace(defaultDetail)
                ? finalDetail
                : $"{defaultDetail} | {finalDetail}",
            Step = step,
            FileLineId = fileLineId,
            OperationId = operationId,
            EvaluationId = evaluationId,
            Severity = DetermineSeverity(code)
        };
    }

    public static ReconciliationErrorDetail Create(
        string code,
        string message,
        string step,
        Guid? fileLineId = null,
        Guid? operationId = null,
        Guid? evaluationId = null,
        string? detail = null,
        string severity = "Error")
    {
        return new ReconciliationErrorDetail
        {
            Code = code,
            Message = message,
            Detail = detail,
            Step = step,
            FileLineId = fileLineId,
            OperationId = operationId,
            EvaluationId = evaluationId,
            Severity = severity
        };
    }

    private static (string Code, string Message, string? Detail) ClassifyException(Exception ex)
    {
        return ex switch
        {
            OperationCanceledException oce =>
                ("OPERATION_CANCELLED", "Reconciliation operation was cancelled.", oce.Message),

            InvalidOperationException ioe =>
                ("INVALID_OPERATION", $"Invalid operation: {ioe.Message}", ioe.InnerException?.Message),

            ArgumentException ae =>
                ("INVALID_ARGUMENT", $"Invalid argument: {ae.Message}", ae.InnerException?.Message),

            FormatException fe =>
                ("FORMAT_ERROR", $"Format error: {fe.Message}", fe.InnerException?.Message),

            TimeoutException te =>
                ("TIMEOUT_ERROR", "Reconciliation operation timed out.", te.Message),

            _ =>
                ("INTERNAL_ERROR", "Unexpected reconciliation error occurred.", ex.Message)
        };
    }

    private static string DetermineSeverity(string code)
    {
        return code switch
        {
            "INTERNAL_ERROR" or "TIMEOUT_ERROR" => "Critical",
            _ => "Error"
        };
    }

    private static string BuildDetailMessage(Exception ex)
    {
        var sb = new StringBuilder();
        var current = ex;

        while (current != null)
        {
            if (sb.Length > 0)
                sb.Append(" => ");

            sb.Append(current.GetType().Name);
            sb.Append(": ");
            sb.Append(current.Message);

            current = current.InnerException;
        }

        return sb.ToString();
    }

    private static string BuildUserFacingMessage(string baseMessage, Exception ex)
    {
        var rootCause = GetRootCauseMessage(ex);
        if (string.IsNullOrWhiteSpace(rootCause))
        {
            return Trim(baseMessage, 2000);
        }

        if (baseMessage.Contains(rootCause, StringComparison.OrdinalIgnoreCase))
        {
            return Trim(baseMessage, 2000);
        }

        return Trim($"{baseMessage} Root cause: {rootCause}", 2000);
    }

    private static string GetRootCauseMessage(Exception ex)
    {
        var current = ex;
        while (current.InnerException is not null)
        {
            current = current.InnerException;
        }

        return current.Message;
    }

    private static string Trim(string value, int maxLength)
    {
        return value.Length <= maxLength
            ? value
            : value[..maxLength];
    }
}
