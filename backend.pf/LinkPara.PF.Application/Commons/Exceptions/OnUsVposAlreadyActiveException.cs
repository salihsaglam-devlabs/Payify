using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions;

public class OnUsVposAlreadyActiveException : ApiException
{
    public OnUsVposAlreadyActiveException() : base(ApiErrorCode.OnUsVposAlreadyActive, "OnUsVposAlreadyActive")
    {
    }
}