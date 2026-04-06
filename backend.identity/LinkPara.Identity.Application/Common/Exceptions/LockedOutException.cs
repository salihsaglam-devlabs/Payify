using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Identity.Application.Common.Exceptions;

public class LockedOutException : ApiException
{
    public LockedOutException()
        : base(ApiErrorCode.LockedOut, "Account has been locked!") { }

    public LockedOutException(DateTimeOffset lockOutEndTime)
        : base(ApiErrorCode.LockedOut,
            $"Your account has been locked for {Convert.ToInt32(lockOutEndTime.Subtract(DateTimeOffset.Now).TotalMinutes)} minutes.")
    { }
}