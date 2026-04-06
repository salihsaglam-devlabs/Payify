using System.Runtime.Serialization;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class AmountIntervalIsAlreadyInUseException : ApiException
{
    public AmountIntervalIsAlreadyInUseException()
        : base(ApiErrorCode.AmountIntervalIsAlreadyInUse, "Amount Interval Is Already InUse!")
    {
    }
    
    protected AmountIntervalIsAlreadyInUseException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
    }
}
