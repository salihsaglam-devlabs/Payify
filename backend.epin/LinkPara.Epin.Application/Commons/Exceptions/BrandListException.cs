using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Epin.Application.Commons.Exceptions;

public class BrandListException : ApiException
{
    public BrandListException(string message)
        : base(ApiErrorCode.BrandList, message)
    {
    }
}