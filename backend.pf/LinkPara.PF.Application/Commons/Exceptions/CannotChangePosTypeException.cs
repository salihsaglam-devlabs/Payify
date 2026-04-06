using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions;

public class CannotChangePosTypeException : ApiException
{
    public CannotChangePosTypeException()
        : base(ApiErrorCode.CannotChangePosType, "CannotChangePosType")
    {
    }
}
