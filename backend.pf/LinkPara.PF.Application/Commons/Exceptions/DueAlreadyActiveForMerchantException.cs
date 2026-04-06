using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions;

public class DueAlreadyActiveForMerchantException : ApiException
{
    public DueAlreadyActiveForMerchantException() 
        : base(ApiErrorCode.DueAlreadyActiveForMerchant, "DueAlreadyActiveForMerchant")
    {
    }
}