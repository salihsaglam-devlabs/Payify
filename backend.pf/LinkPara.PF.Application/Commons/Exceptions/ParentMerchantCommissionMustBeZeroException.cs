using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions;

public class ParentMerchantCommissionMustBeZeroException : ApiException
{
    public ParentMerchantCommissionMustBeZeroException() 
        : base(ApiErrorCode.ParentMerchantCommissionMustBeZero, "ParentMerchantCommissionMustBeZero")
    {
    }
}