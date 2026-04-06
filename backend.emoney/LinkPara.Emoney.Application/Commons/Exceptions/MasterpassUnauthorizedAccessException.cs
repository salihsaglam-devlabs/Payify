using LinkPara.SharedModels.Exceptions;
using System.Runtime.Serialization;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class MasterpassUnauthorizedAccessException : ApiException
{
    public MasterpassUnauthorizedAccessException() : base(ApiErrorCode.MasterpassUnauthorizedAccess,
        "MasterpassUnauthorizedAccessException")
    {
    }

    protected MasterpassUnauthorizedAccessException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
