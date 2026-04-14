using System.Runtime.Serialization;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Card.Application.Commons.Exceptions;

[Serializable]
public class ReconciliationFileNotResolvedException : ApiException
{
    public ReconciliationFileNotResolvedException()
        : base(ApiErrorCode.ReconciliationFileNotResolved, "FileNotResolved") { }
    public ReconciliationFileNotResolvedException(string message)
        : base(ApiErrorCode.ReconciliationFileNotResolved, message) { }
    protected ReconciliationFileNotResolvedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class ReconciliationEvaluationContextNotBuiltException : ApiException
{
    public ReconciliationEvaluationContextNotBuiltException()
        : base(ApiErrorCode.ReconciliationEvaluationContextNotBuilt, "EvaluationContextNotBuilt") { }
    public ReconciliationEvaluationContextNotBuiltException(string message)
        : base(ApiErrorCode.ReconciliationEvaluationContextNotBuilt, message) { }
    protected ReconciliationEvaluationContextNotBuiltException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class ReconciliationCurrentCardRowMissingException : ApiException
{
    public ReconciliationCurrentCardRowMissingException()
        : base(ApiErrorCode.ReconciliationCurrentCardRowMissing, "CurrentCardRowMissing") { }
    public ReconciliationCurrentCardRowMissingException(string message)
        : base(ApiErrorCode.ReconciliationCurrentCardRowMissing, message) { }
    protected ReconciliationCurrentCardRowMissingException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class ReconciliationNoEvaluatorRegisteredException : ApiException
{
    public ReconciliationNoEvaluatorRegisteredException()
        : base(ApiErrorCode.ReconciliationNoEvaluatorRegistered, "NoEvaluatorRegistered") { }
    public ReconciliationNoEvaluatorRegisteredException(string message)
        : base(ApiErrorCode.ReconciliationNoEvaluatorRegistered, message) { }
    protected ReconciliationNoEvaluatorRegisteredException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class ReconciliationBranchRequiresManualGateException : ApiException
{
    public ReconciliationBranchRequiresManualGateException()
        : base(ApiErrorCode.ReconciliationBranchRequiresManualGate, "BranchRequiresManualGate") { }
    public ReconciliationBranchRequiresManualGateException(string message)
        : base(ApiErrorCode.ReconciliationBranchRequiresManualGate, message) { }
    protected ReconciliationBranchRequiresManualGateException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class ReconciliationUnsupportedContentTypeException : ApiException
{
    public ReconciliationUnsupportedContentTypeException()
        : base(ApiErrorCode.ReconciliationUnsupportedContentType, "UnsupportedContentType") { }
    public ReconciliationUnsupportedContentTypeException(string message)
        : base(ApiErrorCode.ReconciliationUnsupportedContentType, message) { }
    protected ReconciliationUnsupportedContentTypeException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class ReconciliationOperationPayloadEmptyException : ApiException
{
    public ReconciliationOperationPayloadEmptyException()
        : base(ApiErrorCode.ReconciliationOperationPayloadEmpty, "OperationPayloadEmpty") { }
    public ReconciliationOperationPayloadEmptyException(string message)
        : base(ApiErrorCode.ReconciliationOperationPayloadEmpty, message) { }
    protected ReconciliationOperationPayloadEmptyException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class ReconciliationOperationPayloadDeserializeFailedException : ApiException
{
    public ReconciliationOperationPayloadDeserializeFailedException()
        : base(ApiErrorCode.ReconciliationOperationPayloadDeserializeFailed, "OperationPayloadDeserializeFailed") { }
    public ReconciliationOperationPayloadDeserializeFailedException(string message)
        : base(ApiErrorCode.ReconciliationOperationPayloadDeserializeFailed, message) { }
    protected ReconciliationOperationPayloadDeserializeFailedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class ReconciliationOperationPayloadValueMissingException : ApiException
{
    public ReconciliationOperationPayloadValueMissingException()
        : base(ApiErrorCode.ReconciliationOperationPayloadValueMissing, "OperationPayloadValueMissing") { }
    public ReconciliationOperationPayloadValueMissingException(string message)
        : base(ApiErrorCode.ReconciliationOperationPayloadValueMissing, message) { }
    protected ReconciliationOperationPayloadValueMissingException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class ReconciliationOperationPayloadConversionFailedException : ApiException
{
    public ReconciliationOperationPayloadConversionFailedException()
        : base(ApiErrorCode.ReconciliationOperationPayloadConversionFailed, "OperationPayloadConversionFailed") { }
    public ReconciliationOperationPayloadConversionFailedException(string message)
        : base(ApiErrorCode.ReconciliationOperationPayloadConversionFailed, message) { }
    protected ReconciliationOperationPayloadConversionFailedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class ReconciliationUnsupportedOperationCodeException : ApiException
{
    public ReconciliationUnsupportedOperationCodeException()
        : base(ApiErrorCode.ReconciliationUnsupportedOperationCode, "UnsupportedOperationCode") { }
    public ReconciliationUnsupportedOperationCodeException(string message)
        : base(ApiErrorCode.ReconciliationUnsupportedOperationCode, message) { }
    protected ReconciliationUnsupportedOperationCodeException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class ReconciliationExecuteMaxEvaluationsInvalidException : ApiException
{
    public ReconciliationExecuteMaxEvaluationsInvalidException()
        : base(ApiErrorCode.ReconciliationExecuteMaxEvaluationsInvalid, "ExecuteMaxEvaluationsInvalid") { }
    public ReconciliationExecuteMaxEvaluationsInvalidException(string message)
        : base(ApiErrorCode.ReconciliationExecuteMaxEvaluationsInvalid, message) { }
    protected ReconciliationExecuteMaxEvaluationsInvalidException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class ReconciliationExecuteLeaseSecondsInvalidException : ApiException
{
    public ReconciliationExecuteLeaseSecondsInvalidException()
        : base(ApiErrorCode.ReconciliationExecuteLeaseSecondsInvalid, "ExecuteLeaseSecondsInvalid") { }
    public ReconciliationExecuteLeaseSecondsInvalidException(string message)
        : base(ApiErrorCode.ReconciliationExecuteLeaseSecondsInvalid, message) { }
    protected ReconciliationExecuteLeaseSecondsInvalidException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class ReconciliationEvaluateChunkSizeInvalidException : ApiException
{
    public ReconciliationEvaluateChunkSizeInvalidException()
        : base(ApiErrorCode.ReconciliationEvaluateChunkSizeInvalid, "EvaluateChunkSizeInvalid") { }
    public ReconciliationEvaluateChunkSizeInvalidException(string message)
        : base(ApiErrorCode.ReconciliationEvaluateChunkSizeInvalid, message) { }
    protected ReconciliationEvaluateChunkSizeInvalidException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class ReconciliationEvaluateClaimTimeoutInvalidException : ApiException
{
    public ReconciliationEvaluateClaimTimeoutInvalidException()
        : base(ApiErrorCode.ReconciliationEvaluateClaimTimeoutInvalid, "EvaluateClaimTimeoutInvalid") { }
    public ReconciliationEvaluateClaimTimeoutInvalidException(string message)
        : base(ApiErrorCode.ReconciliationEvaluateClaimTimeoutInvalid, message) { }
    protected ReconciliationEvaluateClaimTimeoutInvalidException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class ReconciliationEvaluateClaimRetryInvalidException : ApiException
{
    public ReconciliationEvaluateClaimRetryInvalidException()
        : base(ApiErrorCode.ReconciliationEvaluateClaimRetryInvalid, "EvaluateClaimRetryInvalid") { }
    public ReconciliationEvaluateClaimRetryInvalidException(string message)
        : base(ApiErrorCode.ReconciliationEvaluateClaimRetryInvalid, message) { }
    protected ReconciliationEvaluateClaimRetryInvalidException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

[Serializable]
public class ReconciliationAlertBatchSizeInvalidException : ApiException
{
    public ReconciliationAlertBatchSizeInvalidException()
        : base(ApiErrorCode.ReconciliationAlertBatchSizeInvalid, "AlertBatchSizeInvalid") { }
    public ReconciliationAlertBatchSizeInvalidException(string message)
        : base(ApiErrorCode.ReconciliationAlertBatchSizeInvalid, message) { }
    protected ReconciliationAlertBatchSizeInvalidException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

