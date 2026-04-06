using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Epin.Application.Commons.Exceptions;

public class StockNotFoundException : ApiException
{
    public StockNotFoundException(int productId)
        : base(ApiErrorCode.StockNotFound, $"StockNotFound ProductId:{productId}")
    {
    }
}