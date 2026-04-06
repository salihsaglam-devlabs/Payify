using System.Runtime.Serialization;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class MaxDocumentSizeExceededException : ApiException
{
    public MaxDocumentSizeExceededException()
        : base(ApiErrorCode.MaxDocumentSizeExceeded, "MaxDocumentSizeExceeded")
    {
    }

    public MaxDocumentSizeExceededException(object maxFileSize)
        : base(ApiErrorCode.MaxDocumentSizeExceeded, "Document size can not exceed {maxFileSize} MB.")
    {
    }
    
    protected MaxDocumentSizeExceededException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
    }
}