using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Identity.Application.Common.Exceptions;

public class InvalidInputException : ApiException
{
    public InvalidInputException()
        : base(ApiErrorCode.InvalidInput, $"The provided information could not be verified.") { }
}