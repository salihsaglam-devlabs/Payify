using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions;

public class SubMerchantRemoveException : ApiException
{
    public SubMerchantRemoveException() : base(ApiErrorCode.SubMerchantRemove, "SubMerchantRemove")
    {
    }
    public SubMerchantRemoveException(string message) : base(ApiErrorCode.SubMerchantRemove, message)
    { }
}
