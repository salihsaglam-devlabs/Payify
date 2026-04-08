#nullable enable
using System.Text;
using LinkPara.Card.Application.Commons.Interfaces.FileIngestion;
using LinkPara.Card.Application.Commons.Interfaces.Localization;
using LinkPara.Card.Application.Commons.Models.FileIngestion.Contracts.Responses;

namespace LinkPara.Card.Application.Commons.Helpers.FileIngestion;

public sealed class IngestionErrorMapper : IIngestionErrorMapper
{
    private readonly ICardResourceLocalizer _localizer;

    public IngestionErrorMapper(ICardResourceLocalizer localizer)
    {
        _localizer = localizer;
    }

    public IngestionErrorDetail MapException(
        Exception ex,
        string step,
        long? lineNumber = null,
        string? fileName = null,
        string? fieldName = null,
        string? recordType = null,
        string? customMessage = null)
    {
        ArgumentNullException.ThrowIfNull(ex);

        var (code, message, detail) = ClassifyException(ex);

        return new IngestionErrorDetail
        {
            Code = code,
            Message = Trim(customMessage ?? message, 2000),
            Detail = string.IsNullOrWhiteSpace(detail) ? BuildDetailMessage(ex) : $"{detail} | {BuildDetailMessage(ex)}",
            Step = step,
            LineNumber = lineNumber,
            FileName = fileName,
            FieldName = fieldName,
            RecordType = recordType,
            Severity = DetermineSeverity(code)
        };
    }

    public IngestionErrorDetail MapParsingError(
        string fieldName,
        string attemptedValue,
        string expectedFormat,
        long lineNumber,
        string? fileName = null,
        string? recordType = null)
    {
        return new IngestionErrorDetail
        {
            Code = "PARSE_ERROR",
            Message = _localizer.Get("FileIngestion.ParseError", fieldName),
            Detail = _localizer.Get("FileIngestion.ParseErrorDetail", fieldName, attemptedValue, expectedFormat),
            Step = "PARSING",
            LineNumber = lineNumber,
            FileName = fileName,
            FieldName = fieldName,
            RecordType = recordType,
            Severity = "Error"
        };
    }

    public IngestionErrorDetail MapValidationError(
        string validationMessage,
        string? fieldName = null,
        long? lineNumber = null,
        string? fileName = null)
    {
        return new IngestionErrorDetail
        {
            Code = "VALIDATION_ERROR",
            Message = validationMessage,
            Detail = validationMessage,
            Step = "VALIDATION",
            LineNumber = lineNumber,
            FileName = fileName,
            FieldName = fieldName,
            Severity = "Error"
        };
    }

    public IngestionErrorDetail MapDatabaseError(
        Exception ex,
        long? lineNumber = null,
        string? fileName = null)
    {
        ArgumentNullException.ThrowIfNull(ex);

        return new IngestionErrorDetail
        {
            Code = "DATABASE_ERROR",
            Message = _localizer.Get("FileIngestion.DatabaseOperationFailed"),
            Detail = BuildDetailMessage(ex),
            Step = "BULK_INSERT",
            LineNumber = lineNumber,
            FileName = fileName,
            Severity = "Critical"
        };
    }

    public IngestionErrorDetail MapIOError(Exception ex, string fileName)
    {
        ArgumentNullException.ThrowIfNull(ex);

        var message = ex switch
        {
            FileNotFoundException => _localizer.Get("FileIngestion.FileNotFoundForFile", fileName),
            UnauthorizedAccessException => _localizer.Get("FileIngestion.AccessDeniedForFile", fileName),
            IOException => _localizer.Get("FileIngestion.IoErrorForFile", fileName),
            _ => _localizer.Get("FileIngestion.OperationFailedForFile", fileName)
        };

        return new IngestionErrorDetail
        {
            Code = ex switch
            {
                FileNotFoundException => "FILE_NOT_FOUND",
                UnauthorizedAccessException => "ACCESS_DENIED",
                IOException => "IO_ERROR",
                _ => "IO_ERROR"
            },
            Message = message,
            Detail = BuildDetailMessage(ex),
            Step = "FILE_RESOLUTION",
            FileName = fileName,
            Severity = "Error"
        };
    }

    public IngestionErrorDetail MapTransferError(Exception ex, string step, string? fileName = null)
    {
        ArgumentNullException.ThrowIfNull(ex);

        return new IngestionErrorDetail
        {
            Code = "TRANSFER_ERROR",
            Message = _localizer.Get("FileIngestion.TransferFailed"),
            Detail = BuildDetailMessage(ex),
            Step = step,
            FileName = fileName,
            Severity = "Critical"
        };
    }

    private (string code, string message, string? detail) ClassifyException(Exception ex)
    {
        return ex switch
        {
            FileNotFoundException fe => ("FILE_NOT_FOUND", _localizer.Get("FileIngestion.FileNotFound"), fe.Message),
            UnauthorizedAccessException uae => ("ACCESS_DENIED", _localizer.Get("FileIngestion.AccessDenied"), uae.Message),
            IOException ioe => ("IO_ERROR", _localizer.Get("FileIngestion.IoError"), ioe.Message),
            InvalidOperationException ioe => ("INVALID_OPERATION", _localizer.Get("FileIngestion.InvalidOperation"), ioe.InnerException?.Message),
            ArgumentException ae => ("INVALID_ARGUMENT", _localizer.Get("FileIngestion.InvalidArgument"), ae.InnerException?.Message),
            FormatException fe => ("FORMAT_ERROR", _localizer.Get("FileIngestion.FormatError"), fe.InnerException?.Message),
            NotSupportedException nse => ("NOT_SUPPORTED", _localizer.Get("FileIngestion.NotSupported"), nse.InnerException?.Message),
            TimeoutException te => ("TIMEOUT_ERROR", _localizer.Get("FileIngestion.Timeout"), te.Message),
            OperationCanceledException oce => ("OPERATION_CANCELLED", _localizer.Get("Common.OperationCancelled"), oce.Message),
            HttpRequestException hre => ("HTTP_ERROR", _localizer.Get("FileIngestion.HttpError"), hre.Message),
            _ => ("INTERNAL_ERROR", _localizer.Get("FileIngestion.InternalError"), ex.Message)
        };
    }

    private static string DetermineSeverity(string code)
    {
        return code switch
        {
            "INTERNAL_ERROR" or "DATABASE_ERROR" or "TRANSFER_ERROR" => "Critical",
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
