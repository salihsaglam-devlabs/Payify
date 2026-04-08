#nullable enable
using System.Text;
using Microsoft.Extensions.Localization;
using LinkPara.Card.Application.Commons.Interfaces.Reconciliation;
using LinkPara.Card.Application.Commons.Models.Reconciliation;

namespace LinkPara.Card.Application.Commons.Helpers.Reconciliation;

public sealed class ReconciliationErrorMapper : IReconciliationErrorMapper
{
    private readonly IStringLocalizer _localizer;

    public ReconciliationErrorMapper(Func<LinkPara.Card.Application.Commons.Localization.LocalizerResource, IStringLocalizer> localizerFactory)
    {
        _localizer = localizerFactory(LinkPara.Card.Application.Commons.Localization.LocalizerResource.Messages);
    }

    public ReconciliationErrorDetail MapException(
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
        var finalDetail = detail ?? BuildDetailMessage(ex);

        return new ReconciliationErrorDetail
        {
            Code = code,
            Message = Trim(message ?? defaultMessage, 2000),
            Detail = string.IsNullOrWhiteSpace(defaultDetail) ? finalDetail : $"{defaultDetail} | {finalDetail}",
            Step = step,
            FileLineId = fileLineId,
            OperationId = operationId,
            EvaluationId = evaluationId,
            Severity = DetermineSeverity(code)
        };
    }

    public ReconciliationErrorDetail Create(
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

    private (string code, string message, string? detail) ClassifyException(Exception ex)
    {
        return ex switch
        {
            OperationCanceledException oce => ("OPERATION_CANCELLED", _localizer.Get("Reconciliation.OperationCancelled"), oce.Message),
            InvalidOperationException ioe => ("INVALID_OPERATION", _localizer.Get("Reconciliation.InvalidOperation"), ioe.InnerException?.Message),
            ArgumentException ae => ("INVALID_ARGUMENT", _localizer.Get("Reconciliation.InvalidArgument"), ae.InnerException?.Message),
            FormatException fe => ("FORMAT_ERROR", _localizer.Get("Reconciliation.FormatError"), fe.InnerException?.Message),
            TimeoutException te => ("TIMEOUT_ERROR", _localizer.Get("Reconciliation.Timeout"), te.Message),
            _ => ("INTERNAL_ERROR", _localizer.Get("Reconciliation.InternalError"), ex.Message)
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
            {
                sb.Append(" => ");
            }

            sb.Append(current.GetType().Name);
            sb.Append(": ");
            sb.Append(current.Message);
            current = current.InnerException;
        }

        return sb.ToString();
    }

    private static string Trim(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
