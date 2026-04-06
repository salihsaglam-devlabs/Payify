using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Epin.Application.Commons.Exceptions;

public class ProductListException : ApiException
{
    public ProductListException(string message)
        : base(ApiErrorCode.ProdcutList, message)
    {
    }
}
