using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Identity.Application.Common.Exceptions;

public class RemoveUserLockException : ApiException
{
	public RemoveUserLockException() : base(ApiErrorCode.RemoveUserLock,"User lock could not be removed!")
	{
	}
}
