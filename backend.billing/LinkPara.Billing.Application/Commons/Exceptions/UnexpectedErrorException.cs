using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Billing.Application.Commons.Exceptions;

public class UnexpectedErrorException : ApiException
{
    public UnexpectedErrorException() : base(ApiErrorCode.UnexpectedError, "UnexpectedError") { }
}