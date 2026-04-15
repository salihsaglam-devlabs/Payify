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

    // FileIngestion — Configuration Validation
    public const string FileIngestionConfigConnectionsMissing = _prefix + "042";
    public const string FileIngestionConfigProfilesMissing = _prefix + "043";
    public const string FileIngestionConfigProtocolMissing = _prefix + "044";
    public const string FileIngestionConfigSourceMissing = _prefix + "045";
    public const string FileIngestionConfigTargetMissing = _prefix + "046";

    // FileIngestion — SFTP Options Validation
    public const string FileIngestionSftpTimeoutInvalid = _prefix + "047";
    public const string FileIngestionSftpOperationTimeoutInvalid = _prefix + "048";
    public const string FileIngestionSftpRetryCountInvalid = _prefix + "049";

    public const string ReconciliationFileNotResolved = _prefix + "050";
    public const string ReconciliationEvaluationContextNotBuilt = _prefix + "051";
    public const string ReconciliationCurrentCardRowMissing = _prefix + "052";
    
    public const string ReconciliationNoEvaluatorRegistered = _prefix + "053";

    public const string ReconciliationBranchRequiresManualGate = _prefix + "055";
    public const string ReconciliationUnsupportedContentType = _prefix + "056";

    public const string ReconciliationOperationPayloadEmpty = _prefix + "060";
    public const string ReconciliationOperationPayloadDeserializeFailed = _prefix + "061";
    public const string ReconciliationOperationPayloadValueMissing = _prefix + "062";
    public const string ReconciliationOperationPayloadConversionFailed = _prefix + "063";

    public const string ReconciliationUnsupportedOperationCode = _prefix + "070";

    // Reconciliation — Configuration Validation
    public const string ReconciliationExecuteMaxEvaluationsInvalid = _prefix + "073";
    public const string ReconciliationExecuteLeaseSecondsInvalid = _prefix + "074";
    public const string ReconciliationEvaluateChunkSizeInvalid = _prefix + "075";
    public const string ReconciliationEvaluateClaimTimeoutInvalid = _prefix + "076";
    public const string ReconciliationEvaluateClaimRetryInvalid = _prefix + "077";
    public const string ReconciliationAlertBatchSizeInvalid = _prefix + "078";
    public const string ReconciliationEvaluateOperationMaxRetriesInvalid = _prefix + "079";


    // Archive — Configuration Validation
    public const string ArchiveOptionsNotConfigured = _prefix + "086";
    public const string ArchivePreviewLimitInvalid = _prefix + "087";
    public const string ArchiveMaxRunCountInvalid = _prefix + "088";
    public const string ArchiveRetentionDaysInvalid = _prefix + "089";
    public const string ArchiveMinUpdateAgeInvalid = _prefix + "090";

    // Archive — Verification & SQL Dialect
    public const string ArchiveLiveCountsNotCleared = _prefix + "091";
    public const string ArchiveVerificationCountMismatch = _prefix + "092";
    public const string ArchivePropertyNotFound = _prefix + "093";
    public const string ArchivePrimaryKeyNotDefined = _prefix + "094";
    public const string ArchiveEntityTypeNotFound = _prefix + "095";
    public const string ArchiveUnsupportedBeforeDateStrategy = _prefix + "106";
    public const string ArchiveMaxRetryPerFileInvalid = _prefix + "107";
    public const string ArchiveRetryDelaySecondsInvalid = _prefix + "108";

    // FileIngestion — SFTP/FTP Transport
    public const string FileIngestionSftpRetryDelayInvalid = _prefix + "096";
    public const string FileIngestionFtpTimeoutInvalid = _prefix + "097";
    public const string FileIngestionFtpRetryCountInvalid = _prefix + "098";
    public const string FileIngestionFtpRetryDelayInvalid = _prefix + "099";
    public const string FileIngestionFtpRetryExhausted = _prefix + "100";
    public const string FileIngestionFtpUploadRetryExhausted = _prefix + "101";

    // FileIngestion — Processing Validation
    public const string FileIngestionProcessingBatchSizeInvalid = _prefix + "102";
    public const string FileIngestionProcessingRetryBatchSizeInvalid = _prefix + "103";
    public const string FileIngestionProcessingMaxParallelismInvalid = _prefix + "104";

    // FileIngestion — Vault Configuration
    public const string FileIngestionVaultConfigMissing = _prefix + "105";
}
