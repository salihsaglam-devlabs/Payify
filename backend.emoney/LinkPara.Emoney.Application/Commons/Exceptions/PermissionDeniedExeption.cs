using System.Runtime.Serialization;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class PermissionDeniedExeption : ApiException
{
    public PermissionDeniedExeption()
        : base(ApiErrorCode.PermissionDenied, "TimeOut") { }
    
    protected PermissionDeniedExeption(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
    }
}
