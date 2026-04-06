
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Epin.Application.Commons.Exceptions;

public class AlreadyExistsReferenceException : ApiException
{
    public AlreadyExistsReferenceException(string message)
        : base(ApiErrorCode.AlreadyExistsReference, message)
    {
    }
}
