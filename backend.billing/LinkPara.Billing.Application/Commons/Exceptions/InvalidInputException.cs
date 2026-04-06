using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Billing.Application.Commons.Exceptions;

public class InvalidInputException : ApiException
{
    public InvalidInputException() : base(ApiErrorCode.InvalidInput, "InvalidInput") { }
}