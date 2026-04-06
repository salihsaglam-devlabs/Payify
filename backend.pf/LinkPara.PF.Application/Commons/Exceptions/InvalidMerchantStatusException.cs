using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions;

public class InvalidMerchantStatusException : ApiException
{
    public InvalidMerchantStatusException()
        : base(ApiErrorCode.InvalidMerchantStatus, "InvalidMerchantStatus")
    {
    }
}