using System.Runtime.Serialization;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class KPSInformationsNotMatchException : ApiException
{
    public KPSInformationsNotMatchException()
        : base(ApiErrorCode.KPSValidationFailed, "KPS response not matched with the entered information!") { }
    
    protected KPSInformationsNotMatchException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
    }
}