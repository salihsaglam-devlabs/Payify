using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Identity.Application.Common.Exceptions;

public class AlreadyDeactivatedException : ApiException
{
    public AlreadyDeactivatedException()
        : base(ApiErrorCode.AlreadyDeactivated, $"Cannot any process on a deactivated account!") { }
}