using System.Runtime.Serialization;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class ProfileIsPassiveException : ApiException
{
    public ProfileIsPassiveException()
        : base(ApiErrorCode.ProfileIsPassive, "Profile is passive!")
    {
    }
    
    protected ProfileIsPassiveException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
    }
}
