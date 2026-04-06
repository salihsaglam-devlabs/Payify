using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions;

public class DailyReturnAmountCannotGreaterAuthAmountException : ApiException
{
    public DailyReturnAmountCannotGreaterAuthAmountException()
        : base(ApiErrorCode.DailyReturnAmountCannotGreaterAuthAmount, "DailyReturnAmountCannotGreaterAuthAmount")
    {
    }
}