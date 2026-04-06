using System.Runtime.Serialization;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Billing.Application.Commons.Exceptions;

[Serializable]
public class AmountIntervalIsAlreadyInUseException : ApiException
{
    public AmountIntervalIsAlreadyInUseException()
        : base(ApiErrorCode.AmountIntervalIsAlreadyInUseException, "Amount interval is already in use.")
    {
    }

    protected AmountIntervalIsAlreadyInUseException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
