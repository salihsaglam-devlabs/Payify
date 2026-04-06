using System.Runtime.Serialization;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class InvalidKycLevelException : ApiException
{
    public InvalidKycLevelException()
        : base(ApiErrorCode.InvalidKycLevel, "InvalidKycLevel")
    {
    }
    
    protected InvalidKycLevelException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
    }
}
