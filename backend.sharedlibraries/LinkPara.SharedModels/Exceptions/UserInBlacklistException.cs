namespace LinkPara.SharedModels.Exceptions;

public class UserInBlacklistException : CustomApiException
{
    public UserInBlacklistException(string message)
        : base(ErrorCode.UserInBlacklist, message)
    {
    }
}
