using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions;

public class VposNotFoundException : ApiException
{
    public VposNotFoundException() 
        : base(ApiErrorCode.VposNotFound, "MissingMerchantVposException")
    {
    }
}