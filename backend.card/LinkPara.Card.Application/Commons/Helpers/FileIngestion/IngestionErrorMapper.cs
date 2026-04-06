using System.Text;
using LinkPara.Card.Application.Commons.Models.FileIngestion.Contracts.Responses;

namespace LinkPara.Card.Application.Commons.Helpers.FileIngestion;

public static class IngestionErrorMapper
{
    public static IngestionErrorDetail MapException(
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

        var finalMessage = BuildUserFacingMessage(customMessage ?? message, ex);
        var finalDetail = BuildDetailMessage(ex);

        return new IngestionErrorDetail
        {
            Code = code,
            Message = finalMessage,
            Detail = string.IsNullOrWhiteSpace(detail) ? finalDetail : $"{detail} | {finalDetail}",
            Step = step,
            LineNumber = lineNumber,
            FileName = fileName,
            FieldName = fieldName,
            RecordType = recordType,
            Severity = DetermineSeverity(code)
        };
    }

    public static IngestionErrorDetail MapParsingError(
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
            Message = $"Field parsing failed: {fieldName} does not match expected format.",
            Detail = $"Field: {fieldName}, Value: '{attemptedValue}', Expected format: {expectedFormat}",
            Step = "PARSING",
            LineNumber = lineNumber,
            FileName = fileName,
            FieldName = fieldName,
            RecordType = recordType,
            Severity = "Error"
        };
    }

    public static IngestionErrorDetail MapValidationError(
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

    public static IngestionErrorDetail MapDatabaseError(
        Exception ex,
        long? lineNumber = null,
        string? fileName = null)
    {
        ArgumentNullException.ThrowIfNull(ex);

        return new IngestionErrorDetail
        {
            Code = "DATABASE_ERROR",
            Message = BuildUserFacingMessage("Database operation failed.", ex),
            Detail = BuildDetailMessage(ex),
            Step = "BULK_INSERT",
            LineNumber = lineNumber,
            FileName = fileName,
            Severity = "Critical"
        };
    }

    public static IngestionErrorDetail MapIOError(
        Exception ex,
        string fileName)
    {
        ArgumentNullException.ThrowIfNull(ex);

        var message = ex switch
        {
            FileNotFoundException => $"File not found: {fileName}",
            UnauthorizedAccessException => $"Access denied for file: {fileName}",
            IOException => $"IO error occurred while reading file: {fileName}",
            _ => $"File operation failed for: {fileName}"
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
            Message = BuildUserFacingMessage(message, ex),
            Detail = BuildDetailMessage(ex),
            Step = "FILE_RESOLUTION",
            FileName = fileName,
            Severity = "Error"
        };
    }

    public static IngestionErrorDetail MapTransferError(
        Exception ex,
        string step,
        string? fileName = null)
    {
        ArgumentNullException.ThrowIfNull(ex);

        return new IngestionErrorDetail
        {
            Code = "TRANSFER_ERROR",
            Message = BuildUserFacingMessage("File transfer or archive operation failed.", ex),
            Detail = BuildDetailMessage(ex),
            Step = step,
            FileName = fileName,
            Severity = "Critical"
        };
    }

    private static (string code, string message, string? detail) ClassifyException(Exception ex)
    {
        return ex switch
        {
            FileNotFoundException fe =>
                ("FILE_NOT_FOUND", "File not found.", fe.Message),

            UnauthorizedAccessException uae =>
                ("ACCESS_DENIED", "Access denied to file or resource.", uae.Message),

            IOException ioe =>
                ("IO_ERROR", "File input/output error occurred.", ioe.Message),

            InvalidOperationException ioe =>
                ("INVALID_OPERATION", $"Invalid operation: {ioe.Message}", ioe.InnerException?.Message),

            ArgumentException ae =>
                ("INVALID_ARGUMENT", $"Invalid argument: {ae.Message}", ae.InnerException?.Message),

            FormatException fe =>
                ("FORMAT_ERROR", $"Data format error: {fe.Message}", fe.InnerException?.Message),

            NotSupportedException nse =>
                ("NOT_SUPPORTED", $"Operation not supported: {nse.Message}", nse.InnerException?.Message),

            TimeoutException te =>
                ("TIMEOUT_ERROR", "Operation timed out.", te.Message),

            OperationCanceledException oce =>
                ("OPERATION_CANCELLED", "Operation was cancelled.", oce.Message),

            HttpRequestException hre =>
                ("HTTP_ERROR", "HTTP request failed.", hre.Message),

            _ =>
                ("INTERNAL_ERROR", "An unexpected error occurred during processing.", ex.Message)
        };
    }

    private static string DetermineSeverity(string code)
    {
        return code switch
        {
            "INTERNAL_ERROR" or "DATABASE_ERROR" or "TRANSFER_ERROR" => "Critical",
            "IO_ERROR" or "ACCESS_DENIED" or "TIMEOUT_ERROR" or "HTTP_ERROR" => "Error",
            "PARSE_ERROR" or "VALIDATION_ERROR" or "FORMAT_ERROR" => "Error",
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
