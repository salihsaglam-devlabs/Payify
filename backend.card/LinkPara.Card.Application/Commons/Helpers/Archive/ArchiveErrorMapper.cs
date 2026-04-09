#nullable enable
using System.Text;
using Microsoft.Extensions.Localization;
using LinkPara.Card.Application.Commons.Interfaces.Archive;
using LinkPara.Card.Application.Commons.Models.Archive;

namespace LinkPara.Card.Application.Commons.Helpers.Archive;

public sealed class ArchiveErrorMapper : IArchiveErrorMapper
{
    private readonly IStringLocalizer _localizer;

    public ArchiveErrorMapper(Func<Localization.LocalizerResource, IStringLocalizer> localizerFactory)
    {
        _localizer = localizerFactory(Localization.LocalizerResource.Messages);
    }

    public ArchiveErrorDetail MapException(
        Exception ex,
        string step,
        Guid? aggregateId = null,
        string? detail = null,
        string? message = null)
    {
        ArgumentNullException.ThrowIfNull(ex);

        var (code, defaultMessage, defaultDetail) = ClassifyException(ex);
        var finalDetail = detail ?? BuildDetailMessage(ex);

        return new ArchiveErrorDetail
        {
            Code = code,
            Message = Trim(message ?? defaultMessage, 2000),
            Detail = string.IsNullOrWhiteSpace(defaultDetail) ? finalDetail : $"{defaultDetail} | {finalDetail}",
            Step = step,
            AggregateId = aggregateId,
            Severity = DetermineSeverity(code)
        };
    }

    public ArchiveErrorDetail Create(
        string code,
        string message,
        string step,
        Guid? aggregateId = null,
        string? detail = null,
        string severity = "Error")
    {
        return new ArchiveErrorDetail
        {
            Code = code,
            Message = message,
            Detail = detail,
            Step = step,
            AggregateId = aggregateId,
            Severity = severity
        };
    }

    private (string code, string message, string? detail) ClassifyException(Exception ex)
    {
        return ex switch
        {
            OperationCanceledException oce => ("OPERATION_CANCELLED", _localizer.Get("Archive.OperationCancelled"), oce.Message),
            InvalidOperationException ioe => ("INVALID_OPERATION", _localizer.Get("Archive.InvalidOperation"), ioe.InnerException?.Message),
            ArgumentException ae => ("INVALID_ARGUMENT", _localizer.Get("Archive.InvalidArgument"), ae.InnerException?.Message),
            TimeoutException te => ("TIMEOUT_ERROR", _localizer.Get("Archive.Timeout"), te.Message),
            _ => ("INTERNAL_ERROR", _localizer.Get("Archive.InternalError"), ex.Message)
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

