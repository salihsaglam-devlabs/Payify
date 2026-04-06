using System.Runtime.Serialization;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class KpsValidationFailedException : ApiException
{
    public KpsValidationFailedException()
        : base(ApiErrorCode.KPSValidationFailed, "KPS validation failed with the entered information!") { }
    
    protected KpsValidationFailedException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
    }
}