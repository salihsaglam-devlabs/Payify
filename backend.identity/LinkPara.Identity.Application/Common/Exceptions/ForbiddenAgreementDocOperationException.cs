using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Identity.Application.Common.Exceptions;

public class ForbiddenAgreementDocOperationException : ApiException
{
    public ForbiddenAgreementDocOperationException()
        : base(ApiErrorCode.ForbiddenAgreementDocOperation, "First, set \"IsLatest\" statu \"true\" for any version of the agreement!") { }
}