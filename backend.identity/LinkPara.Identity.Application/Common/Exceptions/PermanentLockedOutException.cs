using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Identity.Application.Common.Exceptions;

public class PermanentLockedOutException : ApiException
{
    public PermanentLockedOutException() : base(ApiErrorCode.LockedOut, "Account has been locked!")
    {
    }
}
