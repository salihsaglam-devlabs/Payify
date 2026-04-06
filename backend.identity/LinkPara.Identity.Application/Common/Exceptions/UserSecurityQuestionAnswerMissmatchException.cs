using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Identity.Application.Common.Exceptions;
public class UserSecurityQuestionAnswerMissmatchException : ApiException
{
    public UserSecurityQuestionAnswerMissmatchException() 
        : base(ApiErrorCode.UserSecurityQuestionAnswerMissmatch,  "User Security Question Answer Missmatch !")
    {}
}
