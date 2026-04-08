#nullable enable
using LinkPara.Card.Application.Commons.Models.FileIngestion.Contracts.Responses;

namespace LinkPara.Card.Application.Commons.Interfaces.FileIngestion;

public interface IIngestionErrorMapper
{
    IngestionErrorDetail MapException(
        Exception ex,
        string step,
        long? lineNumber = null,
        string? fileName = null,
        string? fieldName = null,
        string? recordType = null,
        string? customMessage = null);

    IngestionErrorDetail MapParsingError(
        string fieldName,
        string attemptedValue,
        string expectedFormat,
        long lineNumber,
        string? fileName = null,
        string? recordType = null);

    IngestionErrorDetail MapValidationError(
        string validationMessage,
        string? fieldName = null,
        long? lineNumber = null,
        string? fileName = null);

    IngestionErrorDetail MapDatabaseError(
        Exception ex,
        long? lineNumber = null,
        string? fileName = null);

    IngestionErrorDetail MapIOError(
        Exception ex,
        string fileName);

    IngestionErrorDetail MapTransferError(
        Exception ex,
        string step,
        string? fileName = null);
}
