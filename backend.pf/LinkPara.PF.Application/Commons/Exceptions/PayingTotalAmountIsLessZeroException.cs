using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions;

public class PayingTotalAmountIsLessZeroException : ApiException
{
    public PayingTotalAmountIsLessZeroException()
     : base(ApiErrorCode.PayingTotalAmountIsLessZero, "PayingTotalAmountIsLessZero")
    {
    }
}
