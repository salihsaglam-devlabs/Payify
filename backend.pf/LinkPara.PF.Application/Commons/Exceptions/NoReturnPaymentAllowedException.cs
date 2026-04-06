using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions;

public class NoReturnPaymentAllowedException : ApiException
{
    public NoReturnPaymentAllowedException()
        : base(ApiErrorCode.NoReturnPaymentAllowed, "NoReturnPaymentAllowed")
    {
    }
}