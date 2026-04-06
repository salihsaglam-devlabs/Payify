using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Epin.Application.Commons.Exceptions;

public class OrderServiceException : ApiException
{
    public OrderServiceException()
        : base(ApiErrorCode.OrderServiceError, $"OrderServiceError")
    {
    }
}

