using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions;

public class InvalidActivationDateException : ApiException
{
    public InvalidActivationDateException()
        : base(ApiErrorCode.InvalidActivationDate, "InvalidActivationDate")
    {
    }
}
