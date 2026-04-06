using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions;

    public class InternationalCardInstallmentNotAllowedException : ApiException
{
    public InternationalCardInstallmentNotAllowedException()
        : base(ApiErrorCode.InternationalCardInstallmentNotAllowed, "InternationalCardInstallmentNotAllowedException")
    {
    }
}

