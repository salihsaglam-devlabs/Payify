using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Identity.Application.Common.Exceptions;

public class RoleHasUsersException : ApiException
{
    public RoleHasUsersException()
        : base(ApiErrorCode.RoleHasUsersException, $"RoleHasUsersException") { }
}