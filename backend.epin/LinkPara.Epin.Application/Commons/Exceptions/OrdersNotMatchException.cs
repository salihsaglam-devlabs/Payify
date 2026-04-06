using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Epin.Application.Commons.Exceptions;

public class OrdersNotMatchException : ApiException
{
    public OrdersNotMatchException()
        : base(ApiErrorCode.OrdersNotMatch, $"OrdersNotMatch")
    {
    }
}