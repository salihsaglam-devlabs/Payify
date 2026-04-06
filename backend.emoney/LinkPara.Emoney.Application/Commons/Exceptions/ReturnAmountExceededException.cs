using LinkPara.SharedModels.Exceptions;
using System.Runtime.Serialization;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class ReturnAmountExceededException : ApiException
{
    public ReturnAmountExceededException()
    : base(ApiErrorCode.ReturnAmountExceeded, "ReturnAmountExceededException")
    {
    }
    protected ReturnAmountExceededException(SerializationInfo info, StreamingContext context)
    : base(info, context)
    {
    }
}
