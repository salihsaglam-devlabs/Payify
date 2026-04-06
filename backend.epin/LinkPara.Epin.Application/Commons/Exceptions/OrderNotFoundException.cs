
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Epin.Application.Commons.Exceptions;

public class OrderNotFoundException : ApiException
{
    public OrderNotFoundException(string message)
        : base(ApiErrorCode.OrderNotFound, message)
    {
    }
}