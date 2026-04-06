using LinkPara.SharedModels.Exceptions;
namespace LinkPara.Epin.Application.Commons.Exceptions;

public class PerdigitalSystemException : ApiException
{
    public PerdigitalSystemException(string message)
        : base(ApiErrorCode.PerdigitalSystemError, message)
    {
    }
}
