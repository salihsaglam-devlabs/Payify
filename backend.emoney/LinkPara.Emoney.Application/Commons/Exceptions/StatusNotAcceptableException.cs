using System.Runtime.Serialization;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class StatusNotAcceptableException : ApiException
{
    public StatusNotAcceptableException()
        : base(ApiErrorCode.StatusNotAcceptable, "StatusNotAcceptable") { }
    
    protected StatusNotAcceptableException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
    }
}
