using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions;

public class InvalidCommissionRateException : ApiException
{
    public InvalidCommissionRateException() 
        : base(ApiErrorCode.InvalidCommissionRate, "InvalidCommissionRate")
    {
    }
}