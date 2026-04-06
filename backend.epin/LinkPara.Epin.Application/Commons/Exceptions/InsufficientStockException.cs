using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Epin.Application.Commons.Exceptions;

public class InsufficientStockException : ApiException
{
    public InsufficientStockException(string message)
        : base(ApiErrorCode.InsufficientStock, message)
    {
    }
}
