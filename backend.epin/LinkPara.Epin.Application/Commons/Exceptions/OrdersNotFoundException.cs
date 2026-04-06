using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Epin.Application.Commons.Exceptions;

public class OrdersNotFoundException : ApiException
{
    public OrdersNotFoundException()
        : base(ApiErrorCode.OrdersNotFound, $"OrdersNotFound")
    {
    }
}