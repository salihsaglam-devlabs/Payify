#nullable enable
using LinkPara.Card.Application.Commons.Extensions;
using LinkPara.Card.Application.Commons.Interfaces.Reporting;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Shared;
using LinkPara.SharedModels.Exceptions;
using Microsoft.Extensions.Localization;

namespace LinkPara.Card.Application.Commons.Helpers.Reporting;

public sealed class ReportingErrorMapper : IReportingErrorMapper
{
    private readonly IStringLocalizer _localizer;

    public ReportingErrorMapper(Func<Localization.LocalizerResource, IStringLocalizer> localizerFactory)
    {
        _localizer = localizerFactory(Localization.LocalizerResource.Messages);
    }

    public ReconciliationErrorDetail MapException(
        Exception ex,
        string step,
        string? detail = null,
        string? message = null)
    {
        ArgumentNullException.ThrowIfNull(ex);

        var (code, defaultMessage, defaultDetail) = ClassifyException(ex);
        var finalDetail = detail ?? ExceptionDetailHelper.BuildDetailMessage(ex);

        return new ReconciliationErrorDetail
        {
            Code = code,
            Message = Trim(message ?? defaultMessage, 2000),
            Detail = string.IsNullOrWhiteSpace(defaultDetail) ? finalDetail : $"{defaultDetail} | {finalDetail}",
            Step = step,
            Severity = DetermineSeverity(code)
        };
    }

    private (string code, string message, string? detail) ClassifyException(Exception ex)
    {
        return ex switch
        {
            ApiException apiEx => (apiEx.GetType().Name, _localizer.Get("Reporting.ApiError", apiEx.GetType().Name), apiEx.Message),
            OperationCanceledException oce => ("OPERATION_CANCELLED", _localizer.Get("Reporting.OperationCancelled"), oce.Message),
            InvalidOperationException ioe => ("INVALID_OPERATION", _localizer.Get("Reporting.InvalidOperation"), ioe.InnerException?.Message),
            ArgumentException ae => ("INVALID_ARGUMENT", _localizer.Get("Reporting.InvalidArgument"), ae.InnerException?.Message),
            TimeoutException te => ("TIMEOUT_ERROR", _localizer.Get("Reporting.Timeout"), te.Message),
            _ => ("INTERNAL_ERROR", _localizer.Get("Reporting.InternalError"), ex.Message)
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

    private static string Trim(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength];
    }
}

