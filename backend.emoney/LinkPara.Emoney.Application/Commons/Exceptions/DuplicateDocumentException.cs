using LinkPara.SharedModels.Exceptions;
using System.Runtime.Serialization;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class DuplicateDocumentException : ApiException
{
    public DuplicateDocumentException()
        : base(ApiErrorCode.DuplicateDocument, "DuplicateDocument")
    {
    }

    protected DuplicateDocumentException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}