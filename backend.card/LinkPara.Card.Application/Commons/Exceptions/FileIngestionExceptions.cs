using System.Runtime.Serialization;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Card.Application.Commons.Exceptions;

[Serializable]
public class FileIngestionLocalPathNotConfiguredException : ApiException
{
    public FileIngestionLocalPathNotConfiguredException()
        : base(ApiErrorCode.FileIngestionLocalPathNotConfigured, "LocalPathNotConfigured") { }
    public FileIngestionLocalPathNotConfiguredException(string message)
        : base(ApiErrorCode.FileIngestionLocalPathNotConfigured, message) { }
    protected FileIngestionLocalPathNotConfiguredException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionLocalRootPathEmptyException : ApiException
{
    public FileIngestionLocalRootPathEmptyException()
        : base(ApiErrorCode.FileIngestionLocalRootPathEmpty, "LocalRootPathEmpty") { }
    public FileIngestionLocalRootPathEmptyException(string message)
        : base(ApiErrorCode.FileIngestionLocalRootPathEmpty, message) { }
    protected FileIngestionLocalRootPathEmptyException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionFtpPathNotConfiguredException : ApiException
{
    public FileIngestionFtpPathNotConfiguredException()
        : base(ApiErrorCode.FileIngestionFtpPathNotConfigured, "FtpPathNotConfigured") { }
    public FileIngestionFtpPathNotConfiguredException(string message)
        : base(ApiErrorCode.FileIngestionFtpPathNotConfigured, message) { }
    protected FileIngestionFtpPathNotConfiguredException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionSftpPathNotConfiguredException : ApiException
{
    public FileIngestionSftpPathNotConfiguredException()
        : base(ApiErrorCode.FileIngestionSftpPathNotConfigured, "SftpPathNotConfigured") { }
    public FileIngestionSftpPathNotConfiguredException(string message)
        : base(ApiErrorCode.FileIngestionSftpPathNotConfigured, message) { }
    protected FileIngestionSftpPathNotConfiguredException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionProfileNotConfiguredException : ApiException
{
    public FileIngestionProfileNotConfiguredException()
        : base(ApiErrorCode.FileIngestionProfileNotConfigured, "ProfileNotConfigured") { }
    public FileIngestionProfileNotConfiguredException(string message)
        : base(ApiErrorCode.FileIngestionProfileNotConfigured, message) { }
    protected FileIngestionProfileNotConfiguredException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionParsingNotDefinedException : ApiException
{
    public FileIngestionParsingNotDefinedException()
        : base(ApiErrorCode.FileIngestionParsingNotDefined, "ParsingNotDefined") { }
    public FileIngestionParsingNotDefinedException(string message)
        : base(ApiErrorCode.FileIngestionParsingNotDefined, message) { }
    protected FileIngestionParsingNotDefinedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionUnsupportedProtocolException : ApiException
{
    public FileIngestionUnsupportedProtocolException()
        : base(ApiErrorCode.FileIngestionUnsupportedProtocol, "UnsupportedProtocol") { }
    public FileIngestionUnsupportedProtocolException(string message)
        : base(ApiErrorCode.FileIngestionUnsupportedProtocol, message) { }
    protected FileIngestionUnsupportedProtocolException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionConfigConnectionsMissingException : ApiException
{
    public FileIngestionConfigConnectionsMissingException()
        : base(ApiErrorCode.FileIngestionConfigConnectionsMissing, "ConfigConnectionsMissing") { }
    public FileIngestionConfigConnectionsMissingException(string message)
        : base(ApiErrorCode.FileIngestionConfigConnectionsMissing, message) { }
    protected FileIngestionConfigConnectionsMissingException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionConfigProfilesMissingException : ApiException
{
    public FileIngestionConfigProfilesMissingException()
        : base(ApiErrorCode.FileIngestionConfigProfilesMissing, "ConfigProfilesMissing") { }
    public FileIngestionConfigProfilesMissingException(string message)
        : base(ApiErrorCode.FileIngestionConfigProfilesMissing, message) { }
    protected FileIngestionConfigProfilesMissingException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionConfigProtocolMissingException : ApiException
{
    public FileIngestionConfigProtocolMissingException()
        : base(ApiErrorCode.FileIngestionConfigProtocolMissing, "ConfigProtocolMissing") { }
    public FileIngestionConfigProtocolMissingException(string message)
        : base(ApiErrorCode.FileIngestionConfigProtocolMissing, message) { }
    protected FileIngestionConfigProtocolMissingException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionConfigSourceMissingException : ApiException
{
    public FileIngestionConfigSourceMissingException()
        : base(ApiErrorCode.FileIngestionConfigSourceMissing, "ConfigSourceMissing") { }
    public FileIngestionConfigSourceMissingException(string message)
        : base(ApiErrorCode.FileIngestionConfigSourceMissing, message) { }
    protected FileIngestionConfigSourceMissingException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionConfigTargetMissingException : ApiException
{
    public FileIngestionConfigTargetMissingException()
        : base(ApiErrorCode.FileIngestionConfigTargetMissing, "ConfigTargetMissing") { }
    public FileIngestionConfigTargetMissingException(string message)
        : base(ApiErrorCode.FileIngestionConfigTargetMissing, message) { }
    protected FileIngestionConfigTargetMissingException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionVaultConfigMissingException : ApiException
{
    public FileIngestionVaultConfigMissingException()
        : base(ApiErrorCode.FileIngestionVaultConfigMissing, "VaultConfigMissing") { }
    public FileIngestionVaultConfigMissingException(string message)
        : base(ApiErrorCode.FileIngestionVaultConfigMissing, message) { }
    protected FileIngestionVaultConfigMissingException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionParserLineEmptyException : ApiException
{
    public FileIngestionParserLineEmptyException()
        : base(ApiErrorCode.FileIngestionParserLineEmpty, "ParserLineEmpty") { }
    public FileIngestionParserLineEmptyException(string message)
        : base(ApiErrorCode.FileIngestionParserLineEmpty, message) { }
    protected FileIngestionParserLineEmptyException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionParserUnsupportedRecordTypeException : ApiException
{
    public FileIngestionParserUnsupportedRecordTypeException()
        : base(ApiErrorCode.FileIngestionParserUnsupportedRecordType, "ParserUnsupportedRecordType") { }
    public FileIngestionParserUnsupportedRecordTypeException(string message)
        : base(ApiErrorCode.FileIngestionParserUnsupportedRecordType, message) { }
    protected FileIngestionParserUnsupportedRecordTypeException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionHeaderNotResolvedException : ApiException
{
    public FileIngestionHeaderNotResolvedException()
        : base(ApiErrorCode.FileIngestionHeaderNotResolved, "HeaderNotResolved") { }
    public FileIngestionHeaderNotResolvedException(string message)
        : base(ApiErrorCode.FileIngestionHeaderNotResolved, message) { }
    protected FileIngestionHeaderNotResolvedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionFooterNotResolvedException : ApiException
{
    public FileIngestionFooterNotResolvedException()
        : base(ApiErrorCode.FileIngestionFooterNotResolved, "FooterNotResolved") { }
    public FileIngestionFooterNotResolvedException(string message)
        : base(ApiErrorCode.FileIngestionFooterNotResolved, message) { }
    protected FileIngestionFooterNotResolvedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionBoundaryRecordReadFailedException : ApiException
{
    public FileIngestionBoundaryRecordReadFailedException()
        : base(ApiErrorCode.FileIngestionBoundaryRecordReadFailed, "BoundaryRecordReadFailed") { }
    public FileIngestionBoundaryRecordReadFailedException(string message)
        : base(ApiErrorCode.FileIngestionBoundaryRecordReadFailed, message) { }
    public FileIngestionBoundaryRecordReadFailedException(string message, Exception innerException)
        : base(ApiErrorCode.FileIngestionBoundaryRecordReadFailed, message) { }
    protected FileIngestionBoundaryRecordReadFailedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionBoundaryRecordEmptyException : ApiException
{
    public FileIngestionBoundaryRecordEmptyException()
        : base(ApiErrorCode.FileIngestionBoundaryRecordEmpty, "BoundaryRecordEmpty") { }
    public FileIngestionBoundaryRecordEmptyException(string message)
        : base(ApiErrorCode.FileIngestionBoundaryRecordEmpty, message) { }
    protected FileIngestionBoundaryRecordEmptyException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionRecordTypeMismatchException : ApiException
{
    public FileIngestionRecordTypeMismatchException()
        : base(ApiErrorCode.FileIngestionRecordTypeMismatch, "RecordTypeMismatch") { }
    public FileIngestionRecordTypeMismatchException(string message)
        : base(ApiErrorCode.FileIngestionRecordTypeMismatch, message) { }
    protected FileIngestionRecordTypeMismatchException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionDecimalValueInvalidException : ApiException
{
    public FileIngestionDecimalValueInvalidException()
        : base(ApiErrorCode.FileIngestionDecimalValueInvalid, "DecimalValueInvalid") { }
    public FileIngestionDecimalValueInvalidException(string message)
        : base(ApiErrorCode.FileIngestionDecimalValueInvalid, message) { }
    protected FileIngestionDecimalValueInvalidException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionIntValueInvalidException : ApiException
{
    public FileIngestionIntValueInvalidException()
        : base(ApiErrorCode.FileIngestionIntValueInvalid, "IntValueInvalid") { }
    public FileIngestionIntValueInvalidException(string message)
        : base(ApiErrorCode.FileIngestionIntValueInvalid, message) { }
    protected FileIngestionIntValueInvalidException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionLongValueInvalidException : ApiException
{
    public FileIngestionLongValueInvalidException()
        : base(ApiErrorCode.FileIngestionLongValueInvalid, "LongValueInvalid") { }
    public FileIngestionLongValueInvalidException(string message)
        : base(ApiErrorCode.FileIngestionLongValueInvalid, message) { }
    protected FileIngestionLongValueInvalidException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionEnumValueInvalidException : ApiException
{
    public FileIngestionEnumValueInvalidException()
        : base(ApiErrorCode.FileIngestionEnumValueInvalid, "EnumValueInvalid") { }
    public FileIngestionEnumValueInvalidException(string message)
        : base(ApiErrorCode.FileIngestionEnumValueInvalid, message) { }
    protected FileIngestionEnumValueInvalidException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionEntityTypeNotMappedException : ApiException
{
    public FileIngestionEntityTypeNotMappedException()
        : base(ApiErrorCode.FileIngestionEntityTypeNotMapped, "EntityTypeNotMapped") { }
    public FileIngestionEntityTypeNotMappedException(string message)
        : base(ApiErrorCode.FileIngestionEntityTypeNotMapped, message) { }
    protected FileIngestionEntityTypeNotMappedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionTypedSchemaMapperMissingException : ApiException
{
    public FileIngestionTypedSchemaMapperMissingException()
        : base(ApiErrorCode.FileIngestionTypedSchemaMapperMissing, "TypedSchemaMapperMissing") { }
    public FileIngestionTypedSchemaMapperMissingException(string message)
        : base(ApiErrorCode.FileIngestionTypedSchemaMapperMissing, message) { }
    protected FileIngestionTypedSchemaMapperMissingException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionPropertyEmptyException : ApiException
{
    public FileIngestionPropertyEmptyException()
        : base(ApiErrorCode.FileIngestionPropertyEmpty, "PropertyEmpty") { }
    public FileIngestionPropertyEmptyException(string message)
        : base(ApiErrorCode.FileIngestionPropertyEmpty, message) { }
    protected FileIngestionPropertyEmptyException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionPropertyNotMappedException : ApiException
{
    public FileIngestionPropertyNotMappedException()
        : base(ApiErrorCode.FileIngestionPropertyNotMapped, "PropertyNotMapped") { }
    public FileIngestionPropertyNotMappedException(string message)
        : base(ApiErrorCode.FileIngestionPropertyNotMapped, message) { }
    protected FileIngestionPropertyNotMappedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionBulkPropertyMappingNotDefinedException : ApiException
{
    public FileIngestionBulkPropertyMappingNotDefinedException()
        : base(ApiErrorCode.FileIngestionBulkPropertyMappingNotDefined, "BulkPropertyMappingNotDefined") { }
    public FileIngestionBulkPropertyMappingNotDefinedException(string message)
        : base(ApiErrorCode.FileIngestionBulkPropertyMappingNotDefined, message) { }
    protected FileIngestionBulkPropertyMappingNotDefinedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionPropertyNotDefinedException : ApiException
{
    public FileIngestionPropertyNotDefinedException()
        : base(ApiErrorCode.FileIngestionPropertyNotDefined, "PropertyNotDefined") { }
    public FileIngestionPropertyNotDefinedException(string message)
        : base(ApiErrorCode.FileIngestionPropertyNotDefined, message) { }
    protected FileIngestionPropertyNotDefinedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionFooterTxnCountMissingException : ApiException
{
    public FileIngestionFooterTxnCountMissingException()
        : base(ApiErrorCode.FileIngestionFooterTxnCountMissing, "FooterTxnCountMissing") { }
    public FileIngestionFooterTxnCountMissingException(string message)
        : base(ApiErrorCode.FileIngestionFooterTxnCountMissing, message) { }
    protected FileIngestionFooterTxnCountMissingException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionFilePatternMismatchException : ApiException
{
    public FileIngestionFilePatternMismatchException()
        : base(ApiErrorCode.FileIngestionFilePatternMismatch, "FilePatternMismatch") { }
    public FileIngestionFilePatternMismatchException(string message)
        : base(ApiErrorCode.FileIngestionFilePatternMismatch, message) { }
    protected FileIngestionFilePatternMismatchException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionUnsupportedBulkTypeException : ApiException
{
    public FileIngestionUnsupportedBulkTypeException()
        : base(ApiErrorCode.FileIngestionUnsupportedBulkType, "UnsupportedBulkType") { }
    public FileIngestionUnsupportedBulkTypeException(string message)
        : base(ApiErrorCode.FileIngestionUnsupportedBulkType, message) { }
    protected FileIngestionUnsupportedBulkTypeException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionArchiveStatusUpdateRowMismatchException : ApiException
{
    public FileIngestionArchiveStatusUpdateRowMismatchException()
        : base(ApiErrorCode.FileIngestionArchiveStatusUpdateRowMismatch, "ArchiveStatusUpdateRowMismatch") { }
    public FileIngestionArchiveStatusUpdateRowMismatchException(string message)
        : base(ApiErrorCode.FileIngestionArchiveStatusUpdateRowMismatch, message) { }
    protected FileIngestionArchiveStatusUpdateRowMismatchException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionSqlBulkInsertFailedException : ApiException
{
    public FileIngestionSqlBulkInsertFailedException()
        : base(ApiErrorCode.FileIngestionSqlBulkInsertFailed, "SqlBulkInsertFailed") { }
    public FileIngestionSqlBulkInsertFailedException(string message)
        : base(ApiErrorCode.FileIngestionSqlBulkInsertFailed, message) { }
    public FileIngestionSqlBulkInsertFailedException(string message, Exception innerException)
        : base(ApiErrorCode.FileIngestionSqlBulkInsertFailed, message) { }
    protected FileIngestionSqlBulkInsertFailedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionPostgreBulkInsertFailedException : ApiException
{
    public FileIngestionPostgreBulkInsertFailedException()
        : base(ApiErrorCode.FileIngestionPostgreBulkInsertFailed, "PostgreBulkInsertFailed") { }
    public FileIngestionPostgreBulkInsertFailedException(string message)
        : base(ApiErrorCode.FileIngestionPostgreBulkInsertFailed, message) { }
    public FileIngestionPostgreBulkInsertFailedException(string message, Exception innerException)
        : base(ApiErrorCode.FileIngestionPostgreBulkInsertFailed, message) { }
    protected FileIngestionPostgreBulkInsertFailedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionRecordNotFoundFromStartException : ApiException
{
    public FileIngestionRecordNotFoundFromStartException()
        : base(ApiErrorCode.FileIngestionRecordNotFoundFromStart, "RecordNotFoundFromStart") { }
    public FileIngestionRecordNotFoundFromStartException(string message)
        : base(ApiErrorCode.FileIngestionRecordNotFoundFromStart, message) { }
    protected FileIngestionRecordNotFoundFromStartException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionRecordNotFoundFromEndException : ApiException
{
    public FileIngestionRecordNotFoundFromEndException()
        : base(ApiErrorCode.FileIngestionRecordNotFoundFromEnd, "RecordNotFoundFromEnd") { }
    public FileIngestionRecordNotFoundFromEndException(string message)
        : base(ApiErrorCode.FileIngestionRecordNotFoundFromEnd, message) { }
    protected FileIngestionRecordNotFoundFromEndException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionSftpTimeoutInvalidException : ApiException
{
    public FileIngestionSftpTimeoutInvalidException()
        : base(ApiErrorCode.FileIngestionSftpTimeoutInvalid, "SftpTimeoutInvalid") { }
    public FileIngestionSftpTimeoutInvalidException(string message)
        : base(ApiErrorCode.FileIngestionSftpTimeoutInvalid, message) { }
    protected FileIngestionSftpTimeoutInvalidException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionSftpOperationTimeoutInvalidException : ApiException
{
    public FileIngestionSftpOperationTimeoutInvalidException()
        : base(ApiErrorCode.FileIngestionSftpOperationTimeoutInvalid, "SftpOperationTimeoutInvalid") { }
    public FileIngestionSftpOperationTimeoutInvalidException(string message)
        : base(ApiErrorCode.FileIngestionSftpOperationTimeoutInvalid, message) { }
    protected FileIngestionSftpOperationTimeoutInvalidException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionSftpRetryCountInvalidException : ApiException
{
    public FileIngestionSftpRetryCountInvalidException()
        : base(ApiErrorCode.FileIngestionSftpRetryCountInvalid, "SftpRetryCountInvalid") { }
    public FileIngestionSftpRetryCountInvalidException(string message)
        : base(ApiErrorCode.FileIngestionSftpRetryCountInvalid, message) { }
    protected FileIngestionSftpRetryCountInvalidException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionSftpRetryDelayInvalidException : ApiException
{
    public FileIngestionSftpRetryDelayInvalidException()
        : base(ApiErrorCode.FileIngestionSftpRetryDelayInvalid, "SftpRetryDelayInvalid") { }
    public FileIngestionSftpRetryDelayInvalidException(string message)
        : base(ApiErrorCode.FileIngestionSftpRetryDelayInvalid, message) { }
    protected FileIngestionSftpRetryDelayInvalidException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionFtpTimeoutInvalidException : ApiException
{
    public FileIngestionFtpTimeoutInvalidException()
        : base(ApiErrorCode.FileIngestionFtpTimeoutInvalid, "FtpTimeoutInvalid") { }
    public FileIngestionFtpTimeoutInvalidException(string message)
        : base(ApiErrorCode.FileIngestionFtpTimeoutInvalid, message) { }
    protected FileIngestionFtpTimeoutInvalidException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionFtpRetryCountInvalidException : ApiException
{
    public FileIngestionFtpRetryCountInvalidException()
        : base(ApiErrorCode.FileIngestionFtpRetryCountInvalid, "FtpRetryCountInvalid") { }
    public FileIngestionFtpRetryCountInvalidException(string message)
        : base(ApiErrorCode.FileIngestionFtpRetryCountInvalid, message) { }
    protected FileIngestionFtpRetryCountInvalidException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionFtpRetryDelayInvalidException : ApiException
{
    public FileIngestionFtpRetryDelayInvalidException()
        : base(ApiErrorCode.FileIngestionFtpRetryDelayInvalid, "FtpRetryDelayInvalid") { }
    public FileIngestionFtpRetryDelayInvalidException(string message)
        : base(ApiErrorCode.FileIngestionFtpRetryDelayInvalid, message) { }
    protected FileIngestionFtpRetryDelayInvalidException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionFtpRetryExhaustedException : ApiException
{
    public FileIngestionFtpRetryExhaustedException()
        : base(ApiErrorCode.FileIngestionFtpRetryExhausted, "FtpRetryExhausted") { }
    public FileIngestionFtpRetryExhaustedException(string message)
        : base(ApiErrorCode.FileIngestionFtpRetryExhausted, message) { }
    protected FileIngestionFtpRetryExhaustedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionFtpUploadRetryExhaustedException : ApiException
{
    public FileIngestionFtpUploadRetryExhaustedException()
        : base(ApiErrorCode.FileIngestionFtpUploadRetryExhausted, "FtpUploadRetryExhausted") { }
    public FileIngestionFtpUploadRetryExhaustedException(string message)
        : base(ApiErrorCode.FileIngestionFtpUploadRetryExhausted, message) { }
    protected FileIngestionFtpUploadRetryExhaustedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionProcessingBatchSizeInvalidException : ApiException
{
    public FileIngestionProcessingBatchSizeInvalidException()
        : base(ApiErrorCode.FileIngestionProcessingBatchSizeInvalid, "ProcessingBatchSizeInvalid") { }
    public FileIngestionProcessingBatchSizeInvalidException(string message)
        : base(ApiErrorCode.FileIngestionProcessingBatchSizeInvalid, message) { }
    protected FileIngestionProcessingBatchSizeInvalidException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionProcessingRetryBatchSizeInvalidException : ApiException
{
    public FileIngestionProcessingRetryBatchSizeInvalidException()
        : base(ApiErrorCode.FileIngestionProcessingRetryBatchSizeInvalid, "ProcessingRetryBatchSizeInvalid") { }
    public FileIngestionProcessingRetryBatchSizeInvalidException(string message)
        : base(ApiErrorCode.FileIngestionProcessingRetryBatchSizeInvalid, message) { }
    protected FileIngestionProcessingRetryBatchSizeInvalidException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class FileIngestionProcessingMaxParallelismInvalidException : ApiException
{
    public FileIngestionProcessingMaxParallelismInvalidException()
        : base(ApiErrorCode.FileIngestionProcessingMaxParallelismInvalid, "ProcessingMaxParallelismInvalid") { }
    public FileIngestionProcessingMaxParallelismInvalidException(string message)
        : base(ApiErrorCode.FileIngestionProcessingMaxParallelismInvalid, message) { }
    protected FileIngestionProcessingMaxParallelismInvalidException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

