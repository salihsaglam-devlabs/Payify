using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions;

public class InvalidUpdateIksException : ApiException
{
    public InvalidUpdateIksException()
        : base(ApiErrorCode.InvalidUpdateIks, "InvalidUpdateIks")
    {
    }
}
