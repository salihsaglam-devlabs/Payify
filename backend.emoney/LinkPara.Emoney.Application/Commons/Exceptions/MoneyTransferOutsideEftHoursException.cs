using System.Runtime.Serialization;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class MoneyTransferOutsideEftHoursException : ApiException
{
    public MoneyTransferOutsideEftHoursException()
        : base(ApiErrorCode.MoneyTransferOutsideEftHoursException, "MoneyTransferOutsideEftHoursException")
    {
    }
    
    protected MoneyTransferOutsideEftHoursException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
    }
}
