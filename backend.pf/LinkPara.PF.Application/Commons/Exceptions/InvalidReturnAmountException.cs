using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions;

public class InvalidReturnAmountException : ApiException
{
    public InvalidReturnAmountException()
        : base(ApiErrorCode.InvalidReturnAmount, "InvalidReturnAmount")
    {
    }
}