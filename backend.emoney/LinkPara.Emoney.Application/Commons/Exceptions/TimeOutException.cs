using System.Runtime.Serialization;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class TimeOutException : ApiException
{
    public TimeOutException()
        : base(ApiErrorCode.Timeout, "TimeOut") { }
    
    protected TimeOutException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
    }
}
