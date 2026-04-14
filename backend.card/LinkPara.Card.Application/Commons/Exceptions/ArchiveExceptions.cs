using System.Runtime.Serialization;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Card.Application.Commons.Exceptions;

[Serializable]
public class ArchiveOptionsNotConfiguredException : ApiException
{
    public ArchiveOptionsNotConfiguredException()
        : base(ApiErrorCode.ArchiveOptionsNotConfigured, "OptionsNotConfigured") { }
    public ArchiveOptionsNotConfiguredException(string message)
        : base(ApiErrorCode.ArchiveOptionsNotConfigured, message) { }
    protected ArchiveOptionsNotConfiguredException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class ArchivePreviewLimitInvalidException : ApiException
{
    public ArchivePreviewLimitInvalidException()
        : base(ApiErrorCode.ArchivePreviewLimitInvalid, "PreviewLimitInvalid") { }
    public ArchivePreviewLimitInvalidException(string message)
        : base(ApiErrorCode.ArchivePreviewLimitInvalid, message) { }
    protected ArchivePreviewLimitInvalidException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class ArchiveMaxRunCountInvalidException : ApiException
{
    public ArchiveMaxRunCountInvalidException()
        : base(ApiErrorCode.ArchiveMaxRunCountInvalid, "MaxRunCountInvalid") { }
    public ArchiveMaxRunCountInvalidException(string message)
        : base(ApiErrorCode.ArchiveMaxRunCountInvalid, message) { }
    protected ArchiveMaxRunCountInvalidException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class ArchiveRetentionDaysInvalidException : ApiException
{
    public ArchiveRetentionDaysInvalidException()
        : base(ApiErrorCode.ArchiveRetentionDaysInvalid, "RetentionDaysInvalid") { }
    public ArchiveRetentionDaysInvalidException(string message)
        : base(ApiErrorCode.ArchiveRetentionDaysInvalid, message) { }
    protected ArchiveRetentionDaysInvalidException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class ArchiveMinUpdateAgeInvalidException : ApiException
{
    public ArchiveMinUpdateAgeInvalidException()
        : base(ApiErrorCode.ArchiveMinUpdateAgeInvalid, "MinUpdateAgeInvalid") { }
    public ArchiveMinUpdateAgeInvalidException(string message)
        : base(ApiErrorCode.ArchiveMinUpdateAgeInvalid, message) { }
    protected ArchiveMinUpdateAgeInvalidException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class ArchiveLiveCountsNotClearedException : ApiException
{
    public ArchiveLiveCountsNotClearedException()
        : base(ApiErrorCode.ArchiveLiveCountsNotCleared, "LiveCountsNotCleared") { }
    public ArchiveLiveCountsNotClearedException(string message)
        : base(ApiErrorCode.ArchiveLiveCountsNotCleared, message) { }
    protected ArchiveLiveCountsNotClearedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class ArchiveVerificationCountMismatchException : ApiException
{
    public ArchiveVerificationCountMismatchException()
        : base(ApiErrorCode.ArchiveVerificationCountMismatch, "VerificationCountMismatch") { }
    public ArchiveVerificationCountMismatchException(string message)
        : base(ApiErrorCode.ArchiveVerificationCountMismatch, message) { }
    protected ArchiveVerificationCountMismatchException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class ArchivePropertyNotFoundException : ApiException
{
    public ArchivePropertyNotFoundException()
        : base(ApiErrorCode.ArchivePropertyNotFound, "PropertyNotFound") { }
    public ArchivePropertyNotFoundException(string message)
        : base(ApiErrorCode.ArchivePropertyNotFound, message) { }
    protected ArchivePropertyNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class ArchivePrimaryKeyNotDefinedException : ApiException
{
    public ArchivePrimaryKeyNotDefinedException()
        : base(ApiErrorCode.ArchivePrimaryKeyNotDefined, "PrimaryKeyNotDefined") { }
    public ArchivePrimaryKeyNotDefinedException(string message)
        : base(ApiErrorCode.ArchivePrimaryKeyNotDefined, message) { }
    protected ArchivePrimaryKeyNotDefinedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class ArchiveEntityTypeNotFoundException : ApiException
{
    public ArchiveEntityTypeNotFoundException()
        : base(ApiErrorCode.ArchiveEntityTypeNotFound, "EntityTypeNotFound") { }
    public ArchiveEntityTypeNotFoundException(string message)
        : base(ApiErrorCode.ArchiveEntityTypeNotFound, message) { }
    protected ArchiveEntityTypeNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class ArchiveUnsupportedBeforeDateStrategyException : ApiException
{
    public ArchiveUnsupportedBeforeDateStrategyException()
        : base(ApiErrorCode.ArchiveUnsupportedBeforeDateStrategy, "UnsupportedBeforeDateStrategy") { }
    public ArchiveUnsupportedBeforeDateStrategyException(string message)
        : base(ApiErrorCode.ArchiveUnsupportedBeforeDateStrategy, message) { }
    protected ArchiveUnsupportedBeforeDateStrategyException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

