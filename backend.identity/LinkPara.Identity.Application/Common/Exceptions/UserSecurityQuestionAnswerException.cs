using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Identity.Application.Common.Exceptions;
public class UserSecurityQuestionAnswerException : ApiException
{
    public UserSecurityQuestionAnswerException(string message) 
        : base(ApiErrorCode.UserSecurityQuestionAnswer, message)
    {}
}
