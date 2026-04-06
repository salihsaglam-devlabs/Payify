using System.Runtime.Serialization;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class InvalidProfileItemException : ApiException
{
    public InvalidProfileItemException()
        : base(ApiErrorCode.InvalidProfileItem, "Invalid profile item!")
    {
    }
    
    protected InvalidProfileItemException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
    }
}
