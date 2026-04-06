using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Application.Commons.Exceptions;

public class InvalidReferenceNumberException : ApiException
{
    public InvalidReferenceNumberException()
        : base(ApiErrorCode.InvalidReferenceNumber, "InvalidReferenceNumber")
    {
    }
}