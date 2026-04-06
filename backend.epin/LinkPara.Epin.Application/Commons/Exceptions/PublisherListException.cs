using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Epin.Application.Commons.Exceptions;

public class PublisherListException : ApiException
{
    public PublisherListException(string message)
        : base(ApiErrorCode.PublisherList, message)
    {
    }
}
