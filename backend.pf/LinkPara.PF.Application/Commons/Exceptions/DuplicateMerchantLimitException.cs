using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions
{
    public class DuplicateMerchantLimitException : ApiException
    {
        public DuplicateMerchantLimitException() : base(ApiErrorCode.DuplicateMerchantLimit, "DuplicateMerchantLimit")
        {

        }
    }
}
