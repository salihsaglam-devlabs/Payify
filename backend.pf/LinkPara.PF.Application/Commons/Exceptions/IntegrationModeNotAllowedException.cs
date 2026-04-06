using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions;

public class IntegrationModeNotAllowedException : ApiException
{
    public IntegrationModeNotAllowedException()
        : base(ApiErrorCode.IntegrationModeNotAllowed, "IntegrationModeNotAllowed")
    {
    }
}