using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Epin.Application.Commons.Exceptions;

public class HttpFailedException : ApiException
{
    public HttpFailedException(string httpErrorCode)
        : base(ApiErrorCode.HttpFailed, httpErrorCode)
    {
    }
}

