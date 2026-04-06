using System.Runtime.Serialization;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class NotFirstDayOfMonthException : ApiException
{
    public NotFirstDayOfMonthException() 
        : base(ApiErrorCode.NotFirstDayOfMonthException, "Current Date is not beginning of this month!")
    {
    }
    
    protected NotFirstDayOfMonthException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
    }
}