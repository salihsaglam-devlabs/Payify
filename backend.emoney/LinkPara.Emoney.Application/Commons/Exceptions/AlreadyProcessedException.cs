using LinkPara.SharedModels.Exceptions;
using System.Runtime.Serialization;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class AlreadyProcessedException : ApiException
{
    public AlreadyProcessedException() : base(ApiErrorCode.AlreadyProcessed, "AlreadyProcessed")
    {
    }
    protected AlreadyProcessedException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
