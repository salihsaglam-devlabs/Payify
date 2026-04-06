using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions;

public class InvalidPosTypeException : ApiException
{
    public InvalidPosTypeException()
        : base(ApiErrorCode.InvalidPosType, "InvalidPosType")
    {
    }
}
