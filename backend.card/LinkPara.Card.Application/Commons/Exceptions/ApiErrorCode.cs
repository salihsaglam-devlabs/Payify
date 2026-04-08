namespace LinkPara.Card.Application.Commons.Exceptions;

public static class ApiErrorCode
{
    private const string _prefix = "CRD";
    
    public const string FileIngestionLocalPathNotConfigured = _prefix + "001";
    public const string FileIngestionLocalRootPathEmpty = _prefix + "002";
    public const string FileIngestionFtpPathNotConfigured = _prefix + "003";
    public const string FileIngestionSftpPathNotConfigured = _prefix + "004";
    public const string FileIngestionProfileNotConfigured = _prefix + "005";
    public const string FileIngestionParsingNotDefined = _prefix + "006";
    public const string FileIngestionUnsupportedProtocol = _prefix + "007";
    
    public const string FileIngestionParserLineEmpty = _prefix + "010";
    public const string FileIngestionParserUnsupportedRecordType = _prefix + "011";
    public const string FileIngestionHeaderNotResolved = _prefix + "012";
    public const string FileIngestionFooterNotResolved = _prefix + "013";
    public const string FileIngestionBoundaryRecordReadFailed = _prefix + "014";
    public const string FileIngestionBoundaryRecordEmpty = _prefix + "015";
    public const string FileIngestionRecordTypeMismatch = _prefix + "016";

    public const string FileIngestionDecimalValueInvalid = _prefix + "020";
    public const string FileIngestionIntValueInvalid = _prefix + "021";
    public const string FileIngestionLongValueInvalid = _prefix + "022";
    public const string FileIngestionEnumValueInvalid = _prefix + "023";

    public const string FileIngestionEntityTypeNotMapped = _prefix + "027";
    public const string FileIngestionTypedSchemaMapperMissing = _prefix + "028";
    public const string FileIngestionPropertyEmpty = _prefix + "029";
    public const string FileIngestionPropertyNotMapped = _prefix + "030";
    public const string FileIngestionBulkPropertyMappingNotDefined = _prefix + "031";
    public const string FileIngestionPropertyNotDefined = _prefix + "032";
    public const string FileIngestionFooterTxnCountMissing = _prefix + "033";
    
    public const string FileIngestionFilePatternMismatch = _prefix + "035";
    public const string FileIngestionUnsupportedBulkType = _prefix + "036";
    public const string FileIngestionArchiveStatusUpdateRowMismatch = _prefix + "037";
    public const string FileIngestionSqlBulkInsertFailed = _prefix + "038";
    public const string FileIngestionPostgreBulkInsertFailed = _prefix + "039";

    public const string FileIngestionRecordNotFoundFromStart = _prefix + "040";
    public const string FileIngestionRecordNotFoundFromEnd = _prefix + "041";

    public const string ReconciliationFileNotResolved = _prefix + "050";
    public const string ReconciliationEvaluationContextNotBuilt = _prefix + "051";
    public const string ReconciliationCurrentCardRowMissing = _prefix + "052";
    
    public const string ReconciliationNoEvaluatorRegistered = _prefix + "053";
    public const string ReconciliationReviewerNotResolved = _prefix + "054";

    public const string ReconciliationBranchRequiresManualGate = _prefix + "055";
    public const string ReconciliationUnsupportedContentType = _prefix + "056";

    public const string ReconciliationOperationPayloadEmpty = _prefix + "060";
    public const string ReconciliationOperationPayloadDeserializeFailed = _prefix + "061";
    public const string ReconciliationOperationPayloadValueMissing = _prefix + "062";
    public const string ReconciliationOperationPayloadConversionFailed = _prefix + "063";

    public const string ReconciliationUnsupportedOperationCode = _prefix + "070";
    public const string ReconciliationConsumerUnsupportedJobType = _prefix + "071";
    public const string ReconciliationConsumerRequestMissing = _prefix + "072";
}
