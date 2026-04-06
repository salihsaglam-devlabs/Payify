using System.Runtime.Serialization;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class InvalidTierPermissionException : ApiException
{
    public InvalidTierPermissionException()
        : base(ApiErrorCode.InvalidTierPermission, "InvalidTierPermission")
    {
    }
    
    protected InvalidTierPermissionException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
    }
}