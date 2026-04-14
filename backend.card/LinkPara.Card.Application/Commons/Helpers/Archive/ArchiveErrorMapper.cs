#nullable enable
using System.Text;
using LinkPara.Card.Application.Commons.Extensions;
using LinkPara.SharedModels.Exceptions;
using Microsoft.Extensions.Localization;
using LinkPara.Card.Application.Commons.Interfaces.Archive;
using LinkPara.Card.Application.Commons.Models.Archive.Contracts.Responses;

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
        Guid? ingestionFileId = null,
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
            IngestionFileId = ingestionFileId,
            Severity = DetermineSeverity(code)
        };
    }

    public ArchiveErrorDetail Create(
        string code,
        string message,
        string step,
        Guid? ingestionFileId = null,
        string? detail = null,
        string severity = "Error")
    {
        return new ArchiveErrorDetail
        {
            Code = code,
            Message = message,
            Detail = detail,
            Step = step,
            IngestionFileId = ingestionFileId,
            Severity = severity
        };
    }

    private (string code, string message, string? detail) ClassifyException(Exception ex)
    {
        return ex switch
        {
            ApiException apiEx => (apiEx.GetType().Name, _localizer.Get("Archive.ApiError", apiEx.GetType().Name), apiEx.Message),
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
        => ExceptionDetailHelper.BuildDetailMessage(ex);

    private static string Trim(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength];
    }
}

