using System.Runtime.Serialization;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class InvalidTierLevelException : ApiException
{
    public InvalidTierLevelException()
        : base(ApiErrorCode.InvalidTierLevel, "InvalidTierLevel")
    {
    }
    
    protected InvalidTierLevelException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
    }
}