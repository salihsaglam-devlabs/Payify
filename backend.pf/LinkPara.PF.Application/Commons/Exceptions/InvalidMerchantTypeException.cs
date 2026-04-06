using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions;

public class InvalidMerchantTypeException : ApiException
{
    public InvalidMerchantTypeException()
        : base(ApiErrorCode.InvalidMerchantType, "InvalidMerchantType")
    {
    }
}