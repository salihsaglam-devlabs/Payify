using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Epin.Application.Commons.Exceptions;

public class ProvisionErrorException : ApiException
{
    public ProvisionErrorException()
        : base(ApiErrorCode.ProvisionError, $"ProvisionError")
    {
    }

    public ProvisionErrorException(string message)
        : base(ApiErrorCode.ProvisionError, message)
    {
    }
}